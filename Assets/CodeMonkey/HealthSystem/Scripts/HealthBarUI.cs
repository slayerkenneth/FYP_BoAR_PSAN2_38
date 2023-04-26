using UnityEngine;
using UnityEngine.UI;

namespace CodeMonkey.HealthSystemCM {

    /// <summary>
    /// Simple UI Health Bar, sets the Image fillAmount based on the linked HealthSystem
    /// Check the Demo scene for a usage example
    /// </summary>
    public class HealthBarUI : MonoBehaviour {

        [Tooltip("Optional; Either assign a reference in the Editor (that implements IGetHealthSystem) or manually call SetHealthSystem()")]
        [SerializeField] private GameObject getHealthSystemGameObject;

        [Tooltip("Image to show the Health Bar, should be set as Fill, the script modifies fillAmount")]
        [SerializeField] private Image image;

        [Tooltip("Image to show the Shield Bar, should be set as Fill, the script modifies fillAmount")]
        [SerializeField] private Image ShieldImage;

        private HealthSystem healthSystem;


        private void Start() {
            if (HealthSystem.TryGetHealthSystem(getHealthSystemGameObject, out HealthSystem healthSystem)) {
                SetHealthSystem(healthSystem);
            }
        }

        /// <summary>
        /// Set the Health System for this Health Bar
        /// </summary>
        public void SetHealthSystem(HealthSystem healthSystem) {
            if (this.healthSystem != null) {
                this.healthSystem.OnHealthChanged -= HealthSystem_OnHealthChanged;
            }
            this.healthSystem = healthSystem;

            UpdateHealthBar();
            UpdateShieldBar();

            healthSystem.OnHealthChanged += HealthSystem_OnHealthChanged;
        }

        /// <summary>
        /// Event fired from the Health System when Health Amount or Shield Amount changes, update Health Bar and shield bar
        /// </summary>
        private void HealthSystem_OnHealthChanged(object sender, System.EventArgs e) {
            UpdateHealthBar();
            UpdateShieldBar();
        }

        /// <summary>
        /// Update Health Bar using the Image fillAmount based on the current Health Amount
        /// </summary>
        private void UpdateHealthBar() {
            image.fillAmount = healthSystem.GetHealthNormalized();
        }

        /// <summary>
        /// Update Health Bar using the Image fillAmount based on the current Health Amount
        /// </summary>
        private void UpdateShieldBar()
        {
            if(ShieldImage!= null) ShieldImage.fillAmount = healthSystem.GetShieldNormalized();
        }

        /// <summary>
        /// Clean up events when this Game Object is destroyed
        /// </summary>
        private void OnDestroy() {
            if(healthSystem == null) return;
            healthSystem.OnHealthChanged -= HealthSystem_OnHealthChanged;
        }

    }

}