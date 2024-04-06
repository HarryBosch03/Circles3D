using System;
using System.Linq;
using Runtime.Damage;
using Runtime.Player;
using Runtime.Util;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Runtime.Weapons
{
    public class Projectile : MonoBehaviour
    {
        private const int IgnoreDamageLayer = 8;

        private const float HomingSpeed = 8f;

        public GameObject hitFX;
        public float baseSize = 0.1f;

        private TrailRenderer trail;
        private LineRenderer sensorLines;
        private LineRenderer lockLines;

        private SpawnArgs args;
        public Vector3 velocity;
        private bool dead;
        private int age;

        private PlayerAvatar homingTarget;

        public PlayerAvatar shooter { get; private set; }

        public static event Action<Projectile, RaycastHit, IDamageable.DamageReport> projectileDealtDamageEvent;

        private void Awake()
        {
            trail = transform.Find<TrailRenderer>("Trail");
            sensorLines = transform.Find<LineRenderer>("Sensor");
            lockLines = transform.Find<LineRenderer>("Lock");
        }

        private void Start()
        {
            if (trail)
            {
                trail.enabled = false;
            }

            sensorLines.enabled = false;
            lockLines.enabled = false;
        }

        public static Projectile Spawn(Projectile prefab, PlayerAvatar shooter, Vector3 position, Vector3 direction, SpawnArgs args)
        {
            direction.Normalize();
            var orientation = Quaternion.LookRotation(direction);

            var pa = Random.Range(-Mathf.PI, Mathf.PI);
            var pd = Random.Range(0f, args.sprayAngle);

            orientation *= Quaternion.Euler(new Vector3(Mathf.Cos(pa), Mathf.Sin(pa)) * pd);
            direction = orientation * Vector3.forward;

            var instance = Instantiate(prefab, position, Quaternion.LookRotation(direction));
            instance.args = args;
            instance.velocity = direction * args.speed;
            instance.shooter = shooter;
            return instance;
        }

        private void FixedUpdate()
        {
            if (trail && age == 1) trail.enabled = true;
            if (args.homing > float.Epsilon && age > 0) Home();

            Collide();
            transform.position += velocity * Time.deltaTime;
            velocity += Physics.gravity * Time.deltaTime;

            if (age > args.lifetime / Time.fixedDeltaTime) DestroyWithStyle();

            age++;
        }

        private void Home()
        {
            if (!homingTarget)
            {
                var scans = 30;
                var visibleScans = 5;

                sensorLines.enabled = true;
                lockLines.enabled = false;

                sensorLines.positionCount = visibleScans * 3;
                sensorLines.useWorldSpace = true;

                for (var i = 0; i < scans; i++)
                {
                    if (i < visibleScans)
                    {
                        sensorLines.SetPosition(3 * i + 0, transform.position);
                        sensorLines.SetPosition(3 * i + 2, transform.position);
                    }

                    var orientation = Quaternion.LookRotation(velocity);
                    var direction = (Vector3)Random.insideUnitCircle;
                    direction.z = 1f;
                    direction.Normalize();
                    direction = orientation * direction;

                    var ray = new Ray(transform.position, direction);
                    if (Physics.Raycast(ray, out var hit))
                    {
                        homingTarget = hit.collider.GetComponentInParent<PlayerAvatar>();
                        if (homingTarget) break;

                        if (i < visibleScans) sensorLines.SetPosition(3 * i + 1, hit.point);
                    }
                    else
                    {
                        if (i < visibleScans) sensorLines.SetPosition(3 * i + 1, transform.position + direction * 500f);
                    }
                }
            }

            if (homingTarget)
            {
                sensorLines.enabled = false;
                lockLines.enabled = true;

                lockLines.positionCount = 2;
                lockLines.useWorldSpace = true;
                lockLines.SetPosition(0, transform.position);
                lockLines.SetPosition(1, homingTarget.movement.center);

                var target = (homingTarget.movement.center - transform.position).normalized;
                velocity += (target * args.speed - velocity) * HomingSpeed * args.homing * Time.deltaTime;
                velocity -= Physics.gravity * Time.deltaTime;
            }
        }

        private void Collide()
        {
            var ray = new Ray(transform.position, velocity);
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

            if (IDamageable.Damage(shooter ? shooter.gameObject : null, hit, args.damage, velocity, out var report))
            {
                projectileDealtDamageEvent?.Invoke(this, hit, report);
            }
            else
            {
                if (args.bounces > 0)
                {
                    args.bounces--;
                    dead = false;

                    velocity = Vector3.Reflect(velocity, hit.normal);
                    transform.position = hit.point;
                }
            }

            if (hitFX) Instantiate(hitFX, hit.point, Quaternion.LookRotation(hit.normal));
            if (!dead) return;

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
            public int bounces;
            public float homing;
            public float lifetime;
        }
    }
}