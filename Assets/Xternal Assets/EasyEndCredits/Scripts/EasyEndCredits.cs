using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class EasyEndCredits : MonoBehaviour {

    [System.Serializable]
    public struct TextPair {
        public string name;
        public Text text;
    }

    //[System.Serializable]
    //public struct ImagePair {
    //    public string name;
    //    public Image image;
    //}

    [System.Serializable]
    public class EventCallback : UnityEvent<string> {

    }

    private class VisibleComponent {
        public Text text = null;
        //public Image image = null;
        public GameObject GameObject { get { return text != null ? text.gameObject : null; } }
        public RectTransform RectTransform { get { return GameObject.GetComponent<RectTransform>(); } }
        public float Height { get { return text != null ? text.preferredHeight : RectTransform.sizeDelta.y; } }

        public VisibleComponent(Text text) {
            this.text = text;
        }
        //public VisibleComponent(Image image) {
        //    this.image = image;
        //}
    }

    public TextPair[] textPrefabs;
    //public ImagePair[] imagePrefabs;
    public TextAsset credits;
    public float scrollSpeed = 30.0f;       // pixels per second
    public float columnSpace = 1.0f;
    public EventCallback eventCallback;
    public float outlineSpeed = 0.2f;


    private Text currentText;
    private string[] creditLines;
    private int currentLineIndex;
    private List<VisibleComponent> visibleComponents = new List<VisibleComponent>();
    private float nextShowTime;
    private RectTransform rectTransform;

    public float PlayTime { get; private set; }
    public bool IsPlaying { get; private set; }
    public bool IsPaused { get; private set; }

    private Text getTextPrefab(string name) {
        foreach (var pair in textPrefabs) {
            if (pair.name == name) {
                return pair.text;
            }
        }
        return null;
    }

    //private Image getImagePrefab(string name) {
    //    foreach (var pair in imagePrefabs) {
    //        if (pair.name == name) {
    //            return pair.image;
    //        }
    //    }
    //    return null;
    //}

    // Text lighting control.
    private float outlineIntensity = 0.0f;
    private float minOutline = 0.0f;
    private float maxOutline = 1.7f;
    private bool isOutlineIncreasing = true;

    void Awake() {
        foreach (var prefab in textPrefabs) {
            prefab.text.gameObject.SetActive(false);
        }
        //foreach (var prefab in imagePrefabs) {
        //    prefab.image.gameObject.SetActive(false);
        //}
        rectTransform = GetComponent<RectTransform>();
    }

    public void Play() {
        Stop();
        currentText = textPrefabs[0].text;
        var text = credits.text;
        creditLines = text.Split('\n');
        currentLineIndex = 0;
        IsPlaying = true;
        IsPaused = false;
        PlayTime = 0.0f;
        nextShowTime = 0.0f;
    }

    public void Stop() {
        IsPlaying = false;
        IsPaused = false;
        foreach (var comp in visibleComponents) {
            Destroy(comp.GameObject);
        }
        visibleComponents.Clear();
    }

    public void Pause() {
        if (!IsPlaying) return;
        IsPaused = true;
    }

    public void Resume() {
        if (!IsPlaying) return;
        IsPaused = false;
    }

    void Update() {
        if (!IsPlaying) return;
        if (!IsPaused)
        {
            PlayTime += Time.deltaTime;
            if (nextShowTime <= PlayTime)
            {
                if (currentLineIndex < creditLines.Length)
                {
                    var str = creditLines[currentLineIndex];
                    str = str.Trim();
                    float maxHeight = 0.0f;
                    if (str.Length > 0)
                    {
                        List<Text> texts = new List<Text>();
                        //List<Image> images = new List<Image>();
                        string text = "";
                        string command = "";
                        string commandParam = "";
                        bool inCommand = false;
                        bool inCommandParam = false;
                        int columnCount = 1;
                        foreach (var c in str)
                        {
                            if (inCommand)
                            {
                                if (c == '{')
                                {

                                }
                                else if (c == '}')
                                {
                                    command = command.Trim();
                                    switch (command)
                                    {
                                        case "text":
                                            currentText = getTextPrefab(commandParam);
                                            break;
                                        //case "img":
                                        //    {
                                        //        var image = getImagePrefab(commandParam);
                                        //        var imageObj = Instantiate(image.gameObject);
                                        //        maxHeight = Mathf.Max(maxHeight, image.GetComponent<RectTransform>().sizeDelta.y);
                                        //        images.Add(imageObj.GetComponent<Image>());
                                        //    }
                                        //    break;
                                        case "event":
                                            if (eventCallback != null)
                                            {
                                                eventCallback.Invoke(commandParam);
                                            }
                                            break;
                                    }

                                    inCommand = false;
                                    inCommandParam = false;
                                }
                                else if (c == '"')
                                {
                                    inCommandParam = !inCommandParam;
                                }
                                else
                                {
                                    if (inCommandParam)
                                    {
                                        commandParam += c;
                                    }
                                    else
                                    {
                                        command += c;
                                    }
                                }
                            }
                            else
                            {
                                if (c == '\\')
                                {
                                    inCommand = true;
                                    inCommandParam = false;
                                    command = "";
                                    commandParam = "";
                                }
                                else if (c == '|')
                                {
                                    columnCount++;
                                    text = text.Trim();
                                    if (text.Length > 0)
                                    {
                                        var textObj = Instantiate(currentText.gameObject);
                                        var textComponent = textObj.GetComponent<Text>();
                                        textComponent.text = text;
                                        texts.Add(textComponent);
                                    }
                                    text = "";
                                }
                                else
                                {
                                    text += c;
                                }
                            }
                        }
                        text = text.Trim();
                        if (text.Length > 0)
                        {
                            var textObj = Instantiate(currentText.gameObject);
                            var textComponent = textObj.GetComponent<Text>();
                            textComponent.text = text;
                            texts.Add(textComponent);
                        }
                        for (var i = 0; i < texts.Count; i++)
                        {
                            var textComponent = texts[i];
                            maxHeight = Mathf.Max(maxHeight, textComponent.preferredHeight + textComponent.lineSpacing);
                            var textRectTransform = textComponent.gameObject.GetComponent<RectTransform>();
                            textRectTransform.SetParent(rectTransform);
                            var columnWidth = rectTransform.rect.width / columnCount;
                            var x = i * columnWidth + columnWidth * 0.5f - rectTransform.rect.width * 0.5f;
                            switch (textComponent.alignment)
                            {
                                case TextAnchor.LowerLeft:
                                case TextAnchor.MiddleLeft:
                                case TextAnchor.UpperLeft:
                                    x -= columnWidth * 0.5f - columnSpace;
                                    break;
                                case TextAnchor.LowerRight:
                                case TextAnchor.MiddleRight:
                                case TextAnchor.UpperRight:
                                    x += columnWidth * 0.5f - columnSpace;
                                    break;
                            }
                            var y = -rectTransform.rect.height * 0.5f - textComponent.preferredHeight * 0.5f;
                            textRectTransform.anchoredPosition = new Vector2(x, y);
                            textComponent.gameObject.SetActive(true);
                            visibleComponents.Add(new VisibleComponent(textComponent));
                        }
                        //for (var i = 0; i < images.Count; i++) {
                        //    var imageComponent = images[i];
                        //    var imageRectTransform = imageComponent.gameObject.GetComponent<RectTransform>();
                        //    maxHeight = Mathf.Max(maxHeight, imageRectTransform.sizeDelta.y);
                        //    imageRectTransform.SetParent(rectTransform);
                        //    var columnWidth = rectTransform.rect.width / columnCount;
                        //    var x = i * columnWidth + columnWidth * 0.5f - rectTransform.rect.width * 0.5f;
                        //    var y = -rectTransform.rect.height * 0.5f - imageRectTransform.sizeDelta.y * 0.5f;
                        //    imageRectTransform.anchoredPosition = new Vector2(x, y);
                        //    imageComponent.gameObject.SetActive(true);
                        //    visibleComponents.Add(new VisibleComponent(imageComponent));
                        //}
                    }
                    if (maxHeight == 0.0f)
                    {
                        maxHeight = currentText.preferredHeight + currentText.lineSpacing;
                    }
                    currentLineIndex++;
                    nextShowTime = maxHeight / scrollSpeed + PlayTime;
                }
            }

            var removeComponent = new List<VisibleComponent>();
            foreach (var component in visibleComponents)
            {
                component.RectTransform.anchoredPosition += new Vector2(0.0f, scrollSpeed * Time.deltaTime);
                if (component.RectTransform.anchoredPosition.y >= rectTransform.rect.height * 0.5f + component.Height * 0.5f)
                {
                    removeComponent.Add(component);
                }
            }
            foreach (var component in removeComponent)
            {
                visibleComponents.Remove(component);
                Destroy(component.GameObject);
            }
        }

        // Update the texts' glowing outline.
        OutlineUpdate();
    }

    // Update the texts' glowing outline.
    private void OutlineUpdate()
    {
        // For each of the visible text components.
        foreach (var component in visibleComponents)
        {
            // If this is the last component, pause the credits
            if (component.text.text == "Thank you for playing :)" && visibleComponents.Count == 1 && !IsPaused)
            {
                Pause();
                outlineSpeed = 5.0f;
            }

            // Get the attached Outline component.
            Outline outline = component.text.gameObject.GetComponent<Outline>();

            // If outline is now increasing in intensity.
            if (isOutlineIncreasing == true)
            {
                // Increase the outline's intensity.
                outlineIntensity += Time.deltaTime * outlineSpeed;
                // Update the outline values.
                outline.effectDistance = new Vector2(outlineIntensity, outlineIntensity);

                // If the intensity has reached it's peak.
                if (outlineIntensity >= maxOutline)
                {
                    // Start decreasing outline intensity.
                    isOutlineIncreasing = false;
                }
            }
            // Else, the outline intensity is decreasing.
            else
            {
                // Decrease the outline's intensity.
                outlineIntensity -= Time.deltaTime * outlineSpeed;
                // Update the outline values.
                outline.effectDistance = new Vector2(outlineIntensity, outlineIntensity);

                // If the intensity has reached it's minimum.
                if (outlineIntensity <= minOutline)
                {
                    // Start increasing outline intensity.
                    isOutlineIncreasing = true;
                }
            }
        }
    }
}
