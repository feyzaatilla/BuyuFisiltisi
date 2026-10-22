using UnityEngine;

namespace BuyuFisiltisi.Companion.Sensors
{
    public sealed class SensorDebugView : MonoBehaviour
    {
        [SerializeField] private Transform _target;
        [SerializeField] private float _arriveDistance = 0.6f;
        [SerializeField] private ObstacleSensorSettings _obstacleSettings = new ObstacleSensorSettings();
        [SerializeField] private StuckDetectorSettings _stuckSettings = new StuckDetectorSettings();

        private ObstacleSensor _obstacleSensor;
        private TargetSensor _targetSensor;
        private StuckDetector _stuckDetector;
        private Vector3 _lastPosition;
        private float _speed;

        public ObstacleReading Obstacles => _obstacleSensor?.Current ?? ObstacleReading.Clear;
        public TargetReading Target => _targetSensor?.Current ?? TargetReading.None;
        public bool IsStuck => _stuckDetector != null && _stuckDetector.Current;

        private void Awake()
        {
            _obstacleSensor = new ObstacleSensor(transform, new PhysicsRaycaster(), _obstacleSettings);
            _targetSensor = new TargetSensor(transform, _arriveDistance);
            _stuckDetector = new StuckDetector(() => _speed, _stuckSettings);
            _lastPosition = transform.position;
        }

        private void Update()
        {
            var deltaTime = Time.deltaTime;
            _speed = deltaTime > 0f ? Vector3.Distance(transform.position, _lastPosition) / deltaTime : 0f;
            _lastPosition = transform.position;

            _targetSensor.ArriveDistance = _arriveDistance;
            _targetSensor.SetTarget(_target);

            _obstacleSensor.Sense(deltaTime);
            _targetSensor.Sense(deltaTime);
            _stuckDetector.Sense(deltaTime);
        }

        private void OnGUI()
        {
            if (_obstacleSensor == null)
                return;

            var o = Obstacles;
            var t = Target;
            GUILayout.BeginArea(new Rect(10f, 10f, 320f, 140f), GUI.skin.box);
            GUILayout.Label($"Engel  sol: {o.Left}  orta: {o.Center}  sağ: {o.Right}");
            GUILayout.Label(o.Any ? $"En yakın engel: {o.NearestDistance:0.00} m" : "En yakın engel: yok");
            GUILayout.Label(t.HasTarget ? $"Hedef  mesafe: {t.Distance:0.00} m  açı: {t.Angle:0}°" : "Hedef: yok");
            GUILayout.Label($"Vardı: {t.Arrived}   Takıldı: {IsStuck}");
            GUILayout.EndArea();
        }

        private void OnDrawGizmos()
        {
            if (_obstacleSensor == null)
            {
                var preview = new ObstacleSensor(transform, new PhysicsRaycaster(), _obstacleSettings);
                DrawRays(preview, ObstacleReading.Clear);
                return;
            }

            DrawRays(_obstacleSensor, _obstacleSensor.Current);

            if (_target != null)
            {
                Gizmos.color = Target.Arrived ? Color.cyan : Color.yellow;
                Gizmos.DrawLine(transform.position, _target.position);
                Gizmos.DrawWireSphere(_target.position, _arriveDistance);
            }
        }

        private static void DrawRays(ObstacleSensor sensor, ObstacleReading reading)
        {
            var origin = sensor.Origin;
            DrawRay(origin, sensor.LeftDirection, sensor.Range, reading.Left);
            DrawRay(origin, sensor.CenterDirection, sensor.Range, reading.Center);
            DrawRay(origin, sensor.RightDirection, sensor.Range, reading.Right);
        }

        private static void DrawRay(Vector3 origin, Vector3 direction, float range, bool hit)
        {
            Gizmos.color = hit ? Color.red : Color.green;
            Gizmos.DrawLine(origin, origin + direction * range);
        }
    }
}
