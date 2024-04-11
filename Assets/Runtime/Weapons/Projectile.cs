using System;
using System.Linq;
using Circles3D.Runtime.Damage;
using Circles3D.Runtime.Player;
using Circles3D.Runtime.Util;
using UnityEngine;

namespace Circles3D.Runtime.Weapons
{
    public class Projectile : MonoBehaviour
    {
        private const int IgnoreDamageLayer = 8;

        public GameObject hitFX;
        public float baseSize = 0.1f;

        private TrailRenderer trail;
        private LineRenderer sensorLines;
        private LineRenderer lockLines;

        private SpawnArgs args;
        public Vector3 position;
        public Vector3 velocity;
        private bool dead;
        private int age;

        private Vector3 startPosition;
        private Vector3 interpolationPosition0;
        private Vector3 interpolationPosition1;

        public PlayerAvatar shooter { get; private set; }

        public static event Action<Projectile, RaycastHit, IDamageable.DamageReport> ProjectileDealtDamageEvent;
        public static event Action<Projectile, RaycastHit> ProjectileHitEvent;

        private void Awake()
        {
            trail = transform.Find<TrailRenderer>("Trail");

            sensorLines = transform.Find<LineRenderer>("Sensor");
            lockLines = transform.Find<LineRenderer>("Lock");
        }

        private void Start()
        {
            trail.Clear();
            trail.emitting = false;
            sensorLines.enabled = false;
            lockLines.enabled = false;

            startPosition = position;
            interpolationPosition0 = position;
            interpolationPosition1 = interpolationPosition0;
        }

        public static Projectile[] Spawn(Projectile prefab, PlayerAvatar shooter, Vector3 position, Vector3 direction, SpawnArgs args, int seed)
        {
            var projectiles = new Projectile[args.count];
            direction.Normalize();
            
            var shuffle = new Shuffler(args.damage.damage + args.count + args.bounces + seed);
            
            for (var i = 0; i < projectiles.Length; i++)
            {
                var pa = shuffle.Next(-Mathf.PI, Mathf.PI);
                var pd = shuffle.Next(0f, args.sprayAngle);

                var orientation = Quaternion.LookRotation(direction);
                orientation *= Quaternion.Euler(new Vector3(Mathf.Cos(pa), Mathf.Sin(pa)) * pd);
                var instance = Instantiate(prefab, position, Quaternion.LookRotation(direction));
                instance.args = args;
                instance.position = position;
                instance.velocity = orientation * Vector3.forward * args.speed;
                instance.shooter = shooter;

                projectiles[i] = instance;
            }

            return projectiles;
        }

        private void FixedUpdate()
        {
            if (args.homing > float.Epsilon && age > 0) Home();

            Collide();
            position += velocity * Time.deltaTime;
            velocity += Physics.gravity * Time.deltaTime;

            if (age > args.lifetime / Time.fixedDeltaTime) DestroyWithStyle();

            interpolationPosition1 = interpolationPosition0;
            interpolationPosition0 = position;

            if (age == 1) trail.emitting = true;
            age++;
        }

        private void Update()
        {
            transform.position = Vector3.Lerp(interpolationPosition1, interpolationPosition0, (Time.time - Time.fixedTime) / Time.fixedDeltaTime);
        }

        private void Home()
        {
            if (!shooter) return;

            lockLines.enabled = false;
            
            var ray = new Ray(shooter.view.position + shooter.view.forward, shooter.view.forward);
            if (Physics.Raycast(ray, out var hit))
            {
                var target = (hit.point - position).normalized;
                velocity += (target * args.speed - velocity) * args.homing * Time.deltaTime;
                
                lockLines.enabled = true;
                lockLines.useWorldSpace = true;
                lockLines.SetPosition(0, position);
                lockLines.SetPosition(1, hit.point);
            }
        }

        private void Collide()
        {
            var ray = new Ray(position, velocity);
            var step = velocity.magnitude * Time.deltaTime * 1.01f;

            var mask = ~(1 << IgnoreDamageLayer);
            var collisions = Physics.SphereCastAll(ray, baseSize, step, mask).OrderBy(e => e.distance);

            foreach (var hit in collisions)
            {
                ProcessHit(hit);
                if (dead) break;
            }
        }

        private void ProcessHit(RaycastHit hit)
        {
            if (age < 2 && shooter && hit.collider.transform.IsChildOf(shooter.transform)) return;

            dead = true;
            trail.AddPosition(hit.point);

            if (IDamageable.Damage(shooter ? shooter.gameObject : null, hit, args.damage, velocity, out var report))
            {
                ProjectileDealtDamageEvent?.Invoke(this, hit, report);
            }
            else
            {
                if (args.bounces > 0)
                {
                    args.bounces--;
                    dead = false;

                    velocity = Vector3.Reflect(velocity, hit.normal);
                    position = hit.point + velocity.normalized * 0.01f - velocity * Time.deltaTime;
                }
            }

            ProjectileHitEvent?.Invoke(this, hit);
            
            if (hitFX) Instantiate(hitFX, hit.point, Quaternion.LookRotation(hit.normal));
            if (!dead) return;

            transform.position = hit.point;
            DestroyWithStyle();
        }

        private void DestroyWithStyle()
        {
            if (trail)
            {
                trail.transform.SetParent(null, true);
                Destroy(trail.gameObject, trail.time);
            }

            Destroy(gameObject);
        }

        [Serializable]
        public struct SpawnArgs
        {
            public DamageArgs damage;
            public float speed;
            public float sprayAngle;
            public int count;
            public int bounces;
            public float homing;
            public float lifetime;
        }
    }
}