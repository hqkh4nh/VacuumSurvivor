using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace VacuumSurvivor
{
    /// The authored three-card upgrade chooser. Freezes the game while open and reports the
    /// picked index. Uses unscaled time so it works while the game is paused.
    public class LevelUpPanel : MonoBehaviour
    {
        [SerializeField] GameObject root;
        [SerializeField] Text[] titles = new Text[3];
        [SerializeField] Text[] descs = new Text[3];
        [SerializeField] Button[] buttons = new Button[3];
        Action<int> onPick;

        public bool IsOpen => root != null && root.activeSelf;

        void Awake() => root.SetActive(false);

        public void Show(IReadOnlyList<UpgradeData> options, Action<int> pick)
        {
            onPick = pick;
            for (int i = 0; i < 3; i++)
            {
                bool has = i < options.Count;
                buttons[i].gameObject.SetActive(has);
                if (!has) continue;
                titles[i].text = options[i].title;
                descs[i].text = options[i].description;
            }
            root.SetActive(true);
            Time.timeScale = 0f;
        }

        public void Choose(int index)
        {
            if (!IsOpen || index < 0 || index >= buttons.Length || !buttons[index].gameObject.activeSelf) return;
            root.SetActive(false);
            var callback = onPick;
            onPick = null;
            callback?.Invoke(index);
        }
    }
}
