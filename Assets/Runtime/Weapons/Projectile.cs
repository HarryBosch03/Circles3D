using System;
using System.Linq;
using Circles3D.Runtime.Damage;
using Circles3D.Runtime.Player;
using Circles3D.Runtime.Util;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Circles3D.Runtime.Weapons
{
    public class Projectile : MonoBehaviour
    {
        private const int IgnoreDamageLayer = 8;

        private const float HomingSpeed = 3f;

        public GameObject hitFX;
        public float baseSize = 0.1f;

        private TrailRenderer trail;
        private LineRenderer lockLines;

        private SpawnArgs args;
        public Vector3 position;
        public Vector3 velocity;
        private bool dead;
        private int age;

        private Vector3 startPosition;
        private Vector3 interpolationPosition0;
        private Vector3 interpolationPosition1;

        private PlayerAvatar homingTarget;

        public PlayerAvatar shooter { get; private set; }

        public static event Action<Projectile, RaycastHit, IDamageable.DamageReport> ProjectileDealtDamageEvent;
        public static event Action<Projectile, RaycastHit> ProjectileHitEvent;

        private void Awake()
        {
            trail = transform.Find<TrailRenderer>("Trail");

            lockLines = transform.Find<LineRenderer>("Lock");
        }

        private void Start()
        {
            trail.Clear();
            trail.emitting = false;
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
            var scale = args.damage.baseDamage / 30f;
            trail.widthMultiplier = scale * 0.1f;
            transform.localScale = Vector3.one * scale;
            
            transform.position = Vector3.Lerp(interpolationPosition1, interpolationPosition0, (Time.time - Time.fixedTime) / Time.fixedDeltaTime);
        }

        private void Home()
        {
            if (!homingTarget)
            {
                var scans = 30;

                lockLines.enabled = false;

                for (var i = 0; i < scans; i++)
                {
                    var orientation = Quaternion.LookRotation(velocity);
                    var direction = (Vector3)Random.insideUnitCircle;
                    direction.z = 1f;
                    direction.Normalize();
                    direction = orientation * direction;

                    var ray = new Ray(position, direction);
                    if (Physics.Raycast(ray, out var hit))
                    {
                        homingTarget = hit.collider.GetComponentInParent<PlayerAvatar>();
                        if (homingTarget) break;
                    }
                }
            }

            if (homingTarget)
            {
                var dot = Vector3.Dot(velocity.normalized, homingTarget.movement.center - position);
                var angle = Mathf.Acos(dot) * Mathf.Rad2Deg;
                if (angle > 45f) homingTarget = null;
            }

            if (homingTarget)
            {
                lockLines.enabled = true;

                lockLines.positionCount = 2;
                lockLines.useWorldSpace = true;
                lockLines.SetPosition(0, position);
                lockLines.SetPosition(1, homingTarget.movement.center);

                var target = (homingTarget.movement.center - position).normalized;
                velocity += (target * args.speed - velocity) * HomingSpeed * args.homing * Time.deltaTime;
                velocity -= Physics.gravity * Time.deltaTime;
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
            
            if (hitFX) Instantiate(hitFX, hit.point, Quaternion.LookRotation(Vector3.Reflect(velocity.normalized, hit.normal)));
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