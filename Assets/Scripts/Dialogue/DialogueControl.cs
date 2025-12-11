using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogueControl : MonoBehaviour
{
    [System.Serializable]
    public enum idiom
    {
        pt,
        eng,
        spa
    }

    public idiom language;


   [Header("Components")]
   public GameObject dialogueObj; // janela do dialogo
   public Image profileSprite; //sprite do perfil
   public Text speechText; //texto da fala
   public Text actorNameText; //nome do npc

    [Header("Settings")]
    public float typingSpeed; //velocidade da fala

    //Variaveis de controle
    public bool isShowing; // se a janala está visivel 
    private int index; //qunatidade de etxto que tem dentro da fala
    private string[] sentences;


    public static DialogueControl instance;

    private void Awake()
    {
            instance = this;
    }

    void Start()
    {
        
    }

    
    void Update()
    {
        
    }

    IEnumerator TypeSentence()
    {
        foreach(char letter in sentences[index].ToCharArray())
        {
            speechText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
    }

    //pular para proxima fala
    public void NextSentence()
    {
        if(speechText.text == sentences[index])
        {
            if (index < sentences.Length - 1)
            {
                index++; 
                speechText.text = "";
                StartCoroutine(TypeSentence());
            }
            else 
            {
                speechText.text = "";
                index = 0;
                dialogueObj.SetActive(false);
                sentences = null;
                isShowing = false;
            }
        }
    }

    //chamar a fala do npc
    public void Speech(string[] txt)
    {
        if (!isShowing)
        {
            dialogueObj.SetActive(true);
            sentences = txt;
            StartCoroutine(TypeSentence());
            isShowing = true;
        }
    }
}
