using UnityEngine;

namespace MichaelManor
{
    [DisallowMultipleComponent]
    public sealed class ManorOrreryController : MonoBehaviour
    {
        [SerializeField] private Transform movingAssembly;
        [SerializeField] private Transform planetOrbit;
        [SerializeField] private Transform moonOrbit;
        [SerializeField] private Transform cometOrbit;
        [SerializeField] private Transform planet;
        [SerializeField] private Transform moon;
        [SerializeField] private Transform comet;

        [SerializeField] private float assemblySpeed = 3.5f;
        [SerializeField] private float planetSpeed = 15f;
        [SerializeField] private float moonSpeed = -22f;
        [SerializeField] private float cometSpeed = 10f;

        public void Configure(
            Transform assembly,
            Transform planetTrack,
            Transform moonTrack,
            Transform cometTrack,
            Transform planetBody,
            Transform moonBody,
            Transform cometBody)
        {
            movingAssembly = assembly;
            planetOrbit = planetTrack;
            moonOrbit = moonTrack;
            cometOrbit = cometTrack;
            planet = planetBody;
            moon = moonBody;
            comet = cometBody;
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime;

            Rotate(movingAssembly, assemblySpeed, deltaTime);
            Rotate(planetOrbit, planetSpeed, deltaTime);
            Rotate(moonOrbit, moonSpeed, deltaTime);
            Rotate(cometOrbit, cometSpeed, deltaTime);

            Spin(planet, new Vector3(10f, 34f, 4f), deltaTime);
            Spin(moon, new Vector3(4f, -46f, 8f), deltaTime);
            Spin(comet, new Vector3(28f, 18f, 12f), deltaTime);
        }

        private static void Rotate(Transform target, float degreesPerSecond, float deltaTime)
        {
            if (target != null)
            {
                target.Rotate(Vector3.up, degreesPerSecond * deltaTime, Space.Self);
            }
        }

        private static void Spin(Transform target, Vector3 degreesPerSecond, float deltaTime)
        {
            if (target != null)
            {
                target.Rotate(degreesPerSecond * deltaTime, Space.Self);
            }
        }
    }
}
