using UnityEngine;
using extOSC;
using System;
using System.Collections.Generic;

public class GalaxyBudsReceiver : MonoBehaviour
{
    public string address = "/buds/orientation";
    public int port = 9000;
    public Transform target;

    private OSCReceiver receiver;
    private Quaternion calibrationOffset = Quaternion.identity;
    private bool isCalibrated = false;

    private Quaternion lastReceivedQuat = Quaternion.identity;

    // Liste des mappings dynamiquement générés
    private List<Func<float, float, float, float, Quaternion>> mappings = new();
    private List<string> mappingLabels = new();
    private int currentMappingIndex = 0;

    void Start()
    {
        GenerateMappings();

        // Recharger le mapping sauvegardé
        currentMappingIndex = PlayerPrefs.GetInt("mapping_index", currentMappingIndex);

        // Recharger la calibration si elle existe
        if (PlayerPrefs.HasKey("calib_x"))
        {
            calibrationOffset = new Quaternion(
                PlayerPrefs.GetFloat("calib_x"),
                PlayerPrefs.GetFloat("calib_y"),
                PlayerPrefs.GetFloat("calib_z"),
                PlayerPrefs.GetFloat("calib_w")
            );
            isCalibrated = true;
            Debug.Log("[OSC] ✅ Calibration rechargée depuis PlayerPrefs.");
        }

        Debug.Log($"[OSC] ▶️ Mapping initial : {mappingLabels[currentMappingIndex]}");
        Debug.Log("[OSC] ⏳ Appuie sur 'C' pour calibrer, 'Espace' pour tester les mappings, 'V' pour valider.");

        receiver = gameObject.AddComponent<OSCReceiver>();
        receiver.LocalPort = port;
        receiver.Bind(address, OnQuaternionReceived);
    }

    void Update()
    {
        // Calibration avec C
        if (Input.GetKeyDown(KeyCode.C))
        {
            calibrationOffset = Quaternion.Inverse(lastReceivedQuat);
            isCalibrated = true;

            PlayerPrefs.SetFloat("calib_x", calibrationOffset.x);
            PlayerPrefs.SetFloat("calib_y", calibrationOffset.y);
            PlayerPrefs.SetFloat("calib_z", calibrationOffset.z);
            PlayerPrefs.SetFloat("calib_w", calibrationOffset.w);
            PlayerPrefs.Save();

            Debug.Log("[OSC] ✅ Calibration effectuée et sauvegardée.");
        }

        // Tester les mappings avec Espace
        if (Input.GetKeyDown(KeyCode.Space))
        {
            currentMappingIndex = (currentMappingIndex + 1) % mappings.Count;
            Debug.Log($"[OSC] 🔁 Nouveau mapping : {mappingLabels[currentMappingIndex]}");
        }

        // Valider le mapping avec V
        if (Input.GetKeyDown(KeyCode.V))
        {
            PlayerPrefs.SetInt("mapping_index", currentMappingIndex);
            PlayerPrefs.Save();
            Debug.Log($"[OSC] 📌 Mapping {mappingLabels[currentMappingIndex]} validé et sauvegardé !");
        }
    }

    void OnQuaternionReceived(OSCMessage message)
    {
        if (message.Values.Count < 4 || target == null) return;

        float x = message.Values[0].FloatValue;
        float y = message.Values[1].FloatValue;
        float z = message.Values[2].FloatValue;
        float w = message.Values[3].FloatValue;

        lastReceivedQuat = mappings[currentMappingIndex](x, y, z, w);

        if (!isCalibrated)
        {
            Debug.LogWarning("[OSC] ⚠️ Calibration non effectuée ! Appuie sur 'C' pour corriger la référence.");
            return;
        }

        Quaternion calibrated = calibrationOffset * lastReceivedQuat;
        target.rotation = calibrated;

        Vector3 euler = calibrated.eulerAngles;
        Debug.Log($"[OSC] Orientation {mappingLabels[currentMappingIndex]} → Pitch={euler.x:F1}, Yaw={euler.y:F1}, Roll={euler.z:F1}");
    }

    void GenerateMappings()
    {
        var permutations = new (int, int, int)[]
        {
            (0, 1, 2),
            (0, 2, 1),
            (1, 0, 2),
            (1, 2, 0),
            (2, 0, 1),
            (2, 1, 0)
        };

        var signs = new (int, int, int)[]
        {
            (1, 1, 1),
            (1, 1, -1),
            (1, -1, 1),
            (1, -1, -1),
            (-1, 1, 1),
            (-1, 1, -1),
            (-1, -1, 1),
            (-1, -1, -1)
        };

        foreach (var (i, j, k) in permutations)
        {
            foreach (var (si, sj, sk) in signs)
            {
                mappings.Add((x, y, z, w) =>
                {
                    float[] v = new[] { x, y, z };
                    return new Quaternion(
                        v[i] * si,
                        v[j] * sj,
                        v[k] * sk,
                        w
                    );
                });

                string[] labels = { "x", "y", "z" };
                string label = $"({SignLabel(si)}{labels[i]}, {SignLabel(sj)}{labels[j]}, {SignLabel(sk)}{labels[k]})";
                mappingLabels.Add(label);
            }
        }

        // Forcer le mapping initial à (-z, x, -y) si présent
        int index = mappingLabels.FindIndex(l => l == "(-z, x, -y)");
        if (index >= 0)
            currentMappingIndex = index;
    }

    string SignLabel(int s) => s == 1 ? "+" : "-";
}
