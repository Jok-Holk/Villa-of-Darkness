using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

// ─────────────────────────────────────────────────────────
// DATA
// ─────────────────────────────────────────────────────────

[Serializable]
public class DialogueChoice
{
    public string text;
}

[Serializable]
public class DialogueLine
{
    public string speakerName;
    [TextArea(2, 6)]
    public string text;
    public List<DialogueChoice> choices = new List<DialogueChoice>();

    public bool HasChoices => choices != null && choices.Count > 0;
}

[CreateAssetMenu(menuName = "Dialogue/Dialogue Asset")]
public class DialogueAsset : ScriptableObject
{
    public List<DialogueLine> lines = new List<DialogueLine>();
}

// ─────────────────────────────────────────────────────────
// UI CONTROLLER
// ─────────────────────────────────────────────────────────

public class DialogueUI : MonoBehaviour
{
    [Header("References")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI speakerText;
    public TextMeshProUGUI bodyText;
    public GameObject confirmIcon;          // blinking ▼ arrow
    public Transform choiceContainer;       // parent of choice rows
    public GameObject choicePrefab;         // prefab: TextMeshProUGUI row

    [Header("Typewriter")]
    [Range(0.01f, 0.1f)]
    public float charDelay = 0.03f;

    [Header("Events")]
    public UnityEvent OnDialogueEnd;
    [Tooltip("Fired with the 0-based index of the selected choice")]
    public UnityEvent<int> OnChoiceSelected;

    // ── State ──────────────────────────────────────────────
    DialogueAsset _asset;
    int _lineIndex;
    int _choiceIndex;
    bool _typing;
    bool _waitingForNext;
    bool _waitingForChoice;
    Coroutine _typeRoutine;
    List<TextMeshProUGUI> _choiceRows = new List<TextMeshProUGUI>();

    // ── Input mapping ──────────────────────────────────────
    // Works with both old and new Input System via polling.
    bool PressAdvance()  => Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return);
    bool PressUp()       => Input.GetKeyDown(KeyCode.W)     || Input.GetKeyDown(KeyCode.UpArrow);
    bool PressDown()     => Input.GetKeyDown(KeyCode.S)     || Input.GetKeyDown(KeyCode.DownArrow);

    // ─────────────────────────────────────────────────────
    // PUBLIC API
    // ─────────────────────────────────────────────────────

    public void StartDialogue(DialogueAsset asset)
    {
        _asset      = asset;
        _lineIndex  = -1;
        dialoguePanel.SetActive(true);
        AdvanceLine();
    }

    public void CloseDialogue()
    {
        StopAllCoroutines();
        dialoguePanel.SetActive(false);
        ClearChoices();
        confirmIcon.SetActive(false);
    }

    // ─────────────────────────────────────────────────────
    // UNITY
    // ─────────────────────────────────────────────────────

    void Update()
    {
        if (_asset == null || !dialoguePanel.activeSelf) return;

        if (PressAdvance())
        {
            if (_typing)
            {
                SkipTypewriter();
            }
            else if (_waitingForChoice)
            {
                ConfirmChoice();
            }
            else if (_waitingForNext)
            {
                AdvanceLine();
            }
        }

        if (_waitingForChoice)
        {
            if (PressUp())   NavigateChoice(-1);
            if (PressDown()) NavigateChoice(+1);
        }
    }

    // ─────────────────────────────────────────────────────
    // LINE FLOW
    // ─────────────────────────────────────────────────────

    void AdvanceLine()
    {
        _lineIndex++;

        if (_lineIndex >= _asset.lines.Count)
        {
            CloseDialogue();
            OnDialogueEnd?.Invoke();
            return;
        }

        var line = _asset.lines[_lineIndex];
        speakerText.text = line.speakerName;
        confirmIcon.SetActive(false);
        ClearChoices();

        _waitingForNext   = false;
        _waitingForChoice = false;

        if (_typeRoutine != null) StopCoroutine(_typeRoutine);
        _typeRoutine = StartCoroutine(TypeLine(line));
    }

    IEnumerator TypeLine(DialogueLine line)
    {
        _typing   = true;
        bodyText.text = "";

        foreach (char c in line.text)
        {
            bodyText.text += c;
            yield return new WaitForSeconds(charDelay);
        }

        FinishLine(line);
    }

    void SkipTypewriter()
    {
        if (_typeRoutine != null) StopCoroutine(_typeRoutine);
        var line = _asset.lines[_lineIndex];
        bodyText.text = line.text;
        FinishLine(line);
    }

    void FinishLine(DialogueLine line)
    {
        _typing = false;

        if (line.HasChoices)
        {
            BuildChoices(line.choices);
            _waitingForChoice = true;
        }
        else
        {
            confirmIcon.SetActive(true);
            _waitingForNext = true;
        }
    }

    // ─────────────────────────────────────────────────────
    // CHOICES
    // ─────────────────────────────────────────────────────

    void BuildChoices(List<DialogueChoice> choices)
    {
        ClearChoices();
        _choiceIndex = 0;

        for (int i = 0; i < choices.Count; i++)
        {
            var go  = Instantiate(choicePrefab, choiceContainer);
            var tmp = go.GetComponentInChildren<TextMeshProUGUI>();
            _choiceRows.Add(tmp);
        }

        RefreshChoiceHighlight(choices);
    }

    void NavigateChoice(int delta)
    {
        _choiceIndex = Mathf.Clamp(
            _choiceIndex + delta, 0,
            _asset.lines[_lineIndex].choices.Count - 1);
        RefreshChoiceHighlight(_asset.lines[_lineIndex].choices);
    }

    void RefreshChoiceHighlight(List<DialogueChoice> choices)
    {
        for (int i = 0; i < _choiceRows.Count; i++)
        {
            string prefix = (i == _choiceIndex) ? "▶ " : "  ";
            _choiceRows[i].text = prefix + choices[i].text;
            // Optionally tint the active row:
            _choiceRows[i].color = (i == _choiceIndex)
                ? Color.white
                : new Color(0.6f, 0.6f, 0.6f);
        }
    }

    void ConfirmChoice()
    {
        _waitingForChoice = false;
        int selected = _choiceIndex;
        OnChoiceSelected?.Invoke(selected);
        AdvanceLine();
    }

    void ClearChoices()
    {
        foreach (Transform t in choiceContainer)
            Destroy(t.gameObject);
        _choiceRows.Clear();
    }
}