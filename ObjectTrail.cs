using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectTrail : MonoBehaviour
{

    public float lifetime = 2f;

    public float SnapRate = 1f;

    public float DestroyDelay = 0.3f;


    public float ScaleFactor;

    [Header("Audio")]
    public List<AudioClip> audioClips = new List<AudioClip>();

    // Update is called once per frame
    void Start(){
       StartCoroutine(CreateSnapshot(lifetime));
       Destroy(gameObject, 10f);
    }

    void playWoosh(){
        AudioSource audioSource = gameObject.GetComponent<AudioSource>();
        audioSource.clip = audioClips[0];
        audioSource.Play();
    }

    void impact(){
         AudioSource audioSource = gameObject.GetComponent<AudioSource>();
        audioSource.clip = audioClips[1];
        audioSource.Play();
    }

    IEnumerator CreateSnapshot(float alive){
        Animator anim = gameObject.GetComponent<Animator>();

        while(alive > 0){

            alive-=SnapRate;

            GameObject snap = new GameObject();

            MeshRenderer mr = snap.AddComponent<MeshRenderer>();
            MeshFilter mf = snap.AddComponent<MeshFilter>();

            Mesh mesh = new Mesh();
            mesh = transform.GetComponent<MeshFilter>().mesh;
            mf.mesh = mesh;

            Material mat = new Material(transform.GetComponent<MeshRenderer>().material);
            mr.material = mat;
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            snap.transform.SetPositionAndRotation(transform.position, transform.rotation);
            snap.transform.localScale = transform.localScale;

            StartCoroutine(animMat(mat));

            Destroy(snap, DestroyDelay);

            yield return new WaitForSeconds(SnapRate);
        }
        
    }

    IEnumerator animMat(Material mat){
        float alphaFloat = 1f;
        while(mat.GetFloat("_Alpha") > 0){
            alphaFloat -= 0.1f;
            mat.SetFloat("_Alpha", alphaFloat);
            yield return new WaitForSeconds(0.025f);
        }
        
    }


}
