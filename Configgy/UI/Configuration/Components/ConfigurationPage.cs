using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Configgy.UI
{
    public class ConfigurationPage : MonoBehaviour
    {
        public ConfigurationPage Parent { get; private set; }

        [SerializeField] private Text header;
        [SerializeField] private Text footer;
        [SerializeField] private Button backButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private RectTransform contentBody;
        
        private List<IConfigElement> elements = new List<IConfigElement>();

        public bool preventClosing = false;

        private void Start()
        {
            backButton.onClick.AddListener(Back);
            closeButton.onClick.AddListener(Close);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                EscapePressed();

            if (Input.GetKeyDown(KeyCode.Backspace))
                BackspacePressed();
        }

        private void EscapePressed()
        {
            if (preventClosing)
                return;

            Close();
        }

        private void BackspacePressed()
        {
            if (preventClosing)
                return;

            //Prevent backspace from closing the menu if an input field is focused
            foreach (var inputField in contentBody.GetComponentsInChildren<InputField>(true))
            {
                if (inputField.isFocused)
                    return;
            }

            Back();
        }

        public void SetParent(ConfigurationPage page)
        { 
            Parent = page; 
        }

        public void SetHeader(string headerText)
        {
            this.header.text = $"-- {headerText} --";
        }

        public void SetFooter(string footerText) 
        {
            this.footerText = footerText;
            this.footer.text = footerText.TrimEnd('/', '\n', '\r');
        }

        private string footerText;

        public void AddElement(IConfigElement configElement)
        {
            elements.Add(configElement);
        }

        public void ClearElements()
        {
            elements.Clear();
            DestroyAllElements();
        }

        private void DestroyAllElements()
        {
            Transform[] transforms = contentBody.GetChildren();

            for (int i = 0; i < transforms.Length; i++)
            {
                Transform t = transforms[i];
                transforms[i] = null;
                Destroy(t.gameObject);
            }
        }

        public void Open()
        {
            gameObject.SetActive(true);
        }

        public void Back()
        {
            if (Parent != null)
                Parent.Open();

            gameObject.SetActive(false);
        }

        public void Close()
        {
            gameObject.SetActive(false);
        }

        public void RebuildPage()
        {
            DestroyAllElements();

            foreach(IConfigElement configElement in elements.OrderBy(x=>x.GetDescriptor()?.OrderInList))
            {
                if(configElement == null)
                {
                    ConfiggyPlugin.Log.LogError($"Configgy: Null element in page {footer}");
                    continue;
                }

                try
                {
                    configElement.BuildElement(contentBody);
                } catch (System.Exception e)
                {
                    ConfiggyPlugin.Log.LogException(e);
                }
            }
        }

        private void OnEnable()
        {
            backButton.gameObject.SetActive(Parent != null);
            foreach (IConfigElement configElement in elements.OrderBy(x => x.GetDescriptor()?.OrderInList))
            {
                configElement.OnMenuOpen();
            }
        }

        private void OnDisable()
        {
            foreach (IConfigElement configElement in elements.OrderBy(x => x.GetDescriptor()?.OrderInList))
            {
                configElement.OnMenuClose();
            }
        }
    }
}
