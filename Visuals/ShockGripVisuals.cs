using System.Collections.Generic;
using UnityEngine;

namespace ExtraBattleUpgrades.Visuals;

internal static class ShockGripVisuals
{
    private static readonly Dictionary<int, ShockGripBeamEffect> Effects = new();

    internal static void PlayLocal(PlayerAvatar holder)
    {
        if (holder == null || holder.physGrabber == null)
        {
            return;
        }

        PhysGrabBeam beam = holder.physGrabber.physGrabBeamComponent;
        if (beam == null)
        {
            return;
        }

        int key = holder.GetInstanceID();

        if (!Effects.TryGetValue(key, out ShockGripBeamEffect effect) || effect == null)
        {
            effect = beam.gameObject.AddComponent<ShockGripBeamEffect>();
            effect.Setup(beam);
            Effects[key] = effect;
        }
        
        effect.Play();

    }
}

internal sealed class ShockGripBeamEffect : MonoBehaviour
{
    private const int BoltCount = 5;
    private const int PointsPerBolt = 4;

    private PhysGrabBeam _beam;
    private readonly List<LineRenderer> _bolts = new();
    private float _timer;
    private float _redrawTimer;
    
    private Material _beamMaterial;
    private Color _beamOriginalColor;
    private bool _hasBeamOriginalColor;

    internal void Setup(PhysGrabBeam beam)
    {
        _beam = beam;
        
        if (beam.lineRenderer != null)
        {
            _beamMaterial = beam.lineRenderer.material;

            if (_beamMaterial != null && _beamMaterial.HasProperty("_Color"))
            {
                _beamOriginalColor = _beamMaterial.color;
                _hasBeamOriginalColor = true;
            }
        }

        for (int i = 0; i < BoltCount; i++)
        {
            GameObject obj = new GameObject($"Extra Battle Upgrades Shock Grip Bolt {i}");
            obj.transform.SetParent(beam.transform, false);

            LineRenderer line = obj.AddComponent<LineRenderer>();
            line.positionCount = PointsPerBolt;
            line.useWorldSpace = true;
            line.enabled = false;

            line.material = new Material(Shader.Find("Sprites/Default"));
            line.widthMultiplier = 0.025f;
            line.numCapVertices = 2;
            line.numCornerVertices = 2;

            Color blue = new Color(0.05f, 0.85f, 1f, 0.9f);
            line.startColor = blue;
            line.endColor = new Color(0.25f, 0.95f, 1f, 0.55f);
            line.material.color = blue;

            _bolts.Add(line);
        }
    }

    internal void Play()
    {
        _timer = 0.28f;
        _redrawTimer = 0f;

        foreach (LineRenderer bolt in _bolts)
        {
            bolt.enabled = true;
        }
    }

    private void LateUpdate()
    {
        if (_beam == null || _bolts.Count == 0)
        {
            return;
        }

        if (_timer <= 0f)
        {
            foreach (LineRenderer bolt in _bolts)
            {
                bolt.enabled = false;
            }
            
            RestoreBeamColor();

            return;
        }

        _timer -= Time.deltaTime;
        _redrawTimer -= Time.deltaTime;
        
        float pulse = 0.45f + Mathf.Sin(Time.time * 18f) * 0.25f;
        float fade = Mathf.Clamp01(_timer / 0.28f);
        float alpha = Mathf.Clamp01((0.35f + pulse) * fade);
        
        PulseBeamColor(alpha);

        foreach (LineRenderer bolt in _bolts)
        {
            Color startColor = new Color(0.05f, 0.85f, 1f, alpha);
            Color endColor = new Color(0.25f, 0.95f, 1f, alpha * 0.55f);

            bolt.startColor = startColor;
            bolt.endColor = endColor;

            if (bolt.material != null)
            {
                bolt.material.color = startColor;
            }
        }

        if (_redrawTimer <= 0f)
        {
            _redrawTimer = 0.045f;
            DrawBolts();
        }
    }

    private void DrawBolts()
    {
        if (_beam.PhysGrabPointOrigin == null || _beam.PhysGrabPoint == null || _beam.PhysGrabPointPuller == null)
        {
            Disable();
            return;
        }

        Vector3 start = _beam.PhysGrabPointOrigin.position;
        Vector3 end = _beam.PhysGrabPoint.position;

        Vector3 direction = end - start;
        float distance = direction.magnitude;

        if (distance <= 0.2f)
        {
            Disable();
            return;
        }

        Vector3 forward = direction.normalized;
        Vector3 cameraForward = Camera.main != null ? Camera.main.transform.forward : Vector3.forward;
        Vector3 side = Vector3.Cross(forward, cameraForward).normalized;

        if (side.sqrMagnitude < 0.001f)
        {
            side = Vector3.Cross(forward, Vector3.up).normalized;
        }

        Vector3 up = Vector3.Cross(side, forward).normalized;

        for (int b = 0; b < _bolts.Count; b++)
        {
            LineRenderer bolt = _bolts[b];

            float startT = Random.Range(0.02f, 0.72f);
            float lengthT = Random.Range(0.08f, 0.18f);

            for (int i = 0; i < PointsPerBolt; i++)
            {
                float segmentT = i / (float)(PointsPerBolt - 1);
                float t = Mathf.Clamp01(startT + lengthT * segmentT);

                Vector3 basePoint = CalculateBeamPoint(t);
                
                float radius = Random.Range(0.035f, 0.09f);
                float angle = Random.Range(0f, Mathf.PI * 2f);

                Vector3 ringOffset =
                    side * Mathf.Cos(angle) * radius +
                    up * Mathf.Sin(angle) * radius;

                Vector3 zigzag =
                    side * Random.Range(-0.035f, 0.035f) +
                    up * Random.Range(-0.035f, 0.035f);

                bolt.SetPosition(i, basePoint + ringOffset + zigzag);
            }
        }
    }

    private void Disable()
    {
        foreach (LineRenderer bolt in _bolts)
        {
            bolt.enabled = false;
        }
    }
    
    private Vector3 CalculateBeamPoint(float t)
    {
        Vector3 start = _beam.PhysGrabPointOrigin.position;
        Vector3 end = _beam.PhysGrabPoint.position;

        Vector3 puller = _beam.PhysGrabPointPuller.position;
        _beam.physGrabPointPullerSmoothPosition = Vector3.Lerp(
            _beam.physGrabPointPullerSmoothPosition,
            puller,
            Time.deltaTime * 10f);

        Vector3 control = _beam.physGrabPointPullerSmoothPosition * _beam.CurveStrength;

        return Mathf.Pow(1f - t, 2f) * start
               + 2f * (1f - t) * t * control
               + Mathf.Pow(t, 2f) * end;
    }
    
    private void PulseBeamColor(float alpha)
    {
        if (!_hasBeamOriginalColor || _beamMaterial == null)
        {
            return;
        }

        Color shockWhite = Color.Lerp(
            new Color(0.05f, 0.85f, 1f, _beamOriginalColor.a),
            Color.white,
            0.65f);

        float pulse = Mathf.Sin(Time.time * 22f) * 0.5f + 0.5f;
        float amount = Mathf.Clamp01(alpha * pulse * 0.65f);

        _beamMaterial.color = Color.Lerp(_beamOriginalColor, shockWhite, amount);
    }
    
    private void RestoreBeamColor()
    {
        if (_hasBeamOriginalColor && _beamMaterial != null)
        {
            _beamMaterial.color = _beamOriginalColor;
        }
    }
    
}

// internal static class ShockGripVisuals
// {
//     private static readonly Dictionary<int, ShockGripBeamEffect> Effects = new();
//
//     internal static void Play(PlayerAvatar holder, Vector3 targetPosition)
//     {
//         if (holder == null || holder.physGrabber == null)
//         {
//             return;
//         }
//
//         PhysGrabBeam beam = holder.physGrabber.physGrabBeamComponent;
//         if (beam == null)
//         {
//             return;
//         }
//
//         int key = holder.GetInstanceID();
//
//         if (!Effects.TryGetValue(key, out ShockGripBeamEffect effect) || effect == null)
//         {
//             effect = beam.gameObject.AddComponent<ShockGripBeamEffect>();
//             effect.Setup(beam);
//             Effects[key] = effect;
//         }
//
//         effect.Play(targetPosition);
//     }
// }
//
// internal sealed class ShockGripBeamEffect : MonoBehaviour
// {
//     private const int PointCount = 14;
//
//     private PhysGrabBeam _beam;
//     private LineRenderer _line;
//     private float _timer;
//     private Vector3 _targetPosition;
//
//     internal void Setup(PhysGrabBeam beam)
//     {
//         _beam = beam;
//
//         GameObject obj = new GameObject("Extra Battle Upgrades Shock Grip Beam");
//         obj.transform.SetParent(beam.transform, false);
//
//         _line = obj.AddComponent<LineRenderer>();
//         _line.positionCount = PointCount;
//         _line.useWorldSpace = true;
//         _line.enabled = false;
//
//         LineRenderer source = beam.lineRendererOverCharge != null
//             ? beam.lineRendererOverCharge
//             : beam.lineRenderer;
//
//         // if (source != null)
//         // {
//         //     _line.material = new Material(source.material);
//         //     _line.widthMultiplier = source.widthMultiplier * 0.75f;
//         //     _line.textureMode = source.textureMode;
//         //     _line.alignment = source.alignment;
//         // }
//         //
//         // Color blue = new Color(0.05f, 0.85f, 1f, 1f);
//         // _line.startColor = blue;
//         // _line.endColor = blue;
//         
//         _line.material = new Material(Shader.Find("Sprites/Default"));
//         _line.widthMultiplier = 0.12f;
//         _line.numCapVertices = 4;
//         _line.numCornerVertices = 4;
//
//         Color blue = new Color(0.05f, 0.85f, 1f, 1f);
//         _line.startColor = blue;
//         _line.endColor = blue;
//         _line.material.color = blue;
//
//         if (_line.material != null && _line.material.HasProperty("_Color"))
//         {
//             _line.material.color = blue;
//         }
//     }
//
//     internal void Play(Vector3 targetPosition)
//     {
//         _targetPosition = targetPosition;
//         // _timer = 0.18f;
//         _timer = 1.0f;
//
//         if (_line != null)
//         {
//             _line.enabled = true;
//         }
//     }
//
//     private void LateUpdate()
//     {
//         if (_line == null || _beam == null)
//         {
//             return;
//         }
//
//         if (_timer <= 0f)
//         {
//             _line.enabled = false;
//             return;
//         }
//
//         _timer -= Time.deltaTime;
//         DrawLightning();
//     }
//
//     private void DrawLightning()
//     {
//         // Transform originTransform = _beam.PhysGrabPointOriginLocal != null
//         //     ? _beam.PhysGrabPointOriginLocal
//         //     : _beam.PhysGrabPointOrigin;
//         //
//         // if (originTransform == null)
//         // {
//         //     _line.enabled = false;
//         //     return;
//         // }
//         //
//         // Vector3 start = originTransform.position;
//         // Vector3 end = _targetPosition;
//         
//         if (_beam.PhysGrabPointOrigin == null || _beam.PhysGrabPoint == null)
//         {
//             _line.enabled = false;
//             return;
//         }
//
//         Vector3 start = _beam.PhysGrabPointOrigin.position;
//         Vector3 end = _beam.PhysGrabPoint.position;
//
//         Vector3 direction = end - start;
//         float distance = direction.magnitude;
//
//         if (distance <= 0.1f)
//         {
//             _line.enabled = false;
//             return;
//         }
//
//         Vector3 normal = direction.normalized;
//         Vector3 side = Vector3.Cross(normal, Camera.main != null ? Camera.main.transform.forward : Vector3.up).normalized;
//         Vector3 up = Vector3.Cross(side, normal).normalized;
//
//         for (int i = 0; i < PointCount; i++)
//         {
//             float t = i / (float)(PointCount - 1);
//             Vector3 point = Vector3.Lerp(start, end, t);
//
//             if (i != 0 && i != PointCount - 1)
//             {
//                 float jitter = Mathf.Sin(Time.time * 80f + i * 13.37f) * 0.16f;
//                 float random = Random.Range(-0.16f, 0.16f);
//                 point += side * (jitter + random);
//                 point += up * Random.Range(-0.12f, 0.12f);
//             }
//
//             _line.SetPosition(i, point);
//         }
//     }
// }