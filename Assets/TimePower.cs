using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;
using UnityEngine.Video;

public class TimePower : MonoBehaviour
{

    public float cooldown = 6f;
    public float invulnTime = 2f;

    public List<TextMeshProUGUI> uiText = new List<TextMeshProUGUI>();
    public TextMeshProUGUI wonderingText;
    public TextMeshProUGUI deathText;
    public List<string> messages;
    public List<Image> uiSprite = new List<Image>();
    public RawImage vid;
    public Image timerHand;
    public Image timerCircle;
    public static List<Rigidbody> playerAndEnemies = new List<Rigidbody>();
    public bool timeSlowed;
    public Color coolDownColor;
    public Color chargedColor;
    public Color pausedColor;
    public Material InvulnFlashMat;
    public Material NormalMat;
    public GameObject DeathCylinder;
    public ParticleSystem forceParticles;

    private float cooldownTimer = 0f;
    private float invulnTimer = 0f;
    private bool onCooldown;
    private Renderer renderer;
    private Coroutine flashingCor;

    public PostProcessVolume vol;

    public float dashTime = 1f;
    public float dashSpeed = 15f;
    public List<GameObject> afterImages = new List<GameObject>();

    private void Start()
    {
        renderer = GetComponent<Renderer>();
        var bloom = ScriptableObject.CreateInstance<Bloom>();
        bloom.enabled.Override(true);
        bloom.intensity.Override(2f);
        var grain = ScriptableObject.CreateInstance<Grain>();
        grain.enabled.Override(true);
        grain.intensity.Override(.6f);
        var chromaticAberration = ScriptableObject.CreateInstance<ChromaticAberration>();
        chromaticAberration.enabled.Override(true);
        chromaticAberration.intensity.Override(.6f);
        var glitch = ScriptableObject.CreateInstance<Glitch>();
        glitch.enabled.Override(true);
        glitch.intensity.Override(1f);
        glitch.colorIntensity.Override(1f);
        var grade = ScriptableObject.CreateInstance<ColorGrading>();
        grade.enabled.Override(true);
        grade.postExposure.Override(-.7f);
        vol = PostProcessManager.instance.QuickVolume(10, 100f, new PostProcessEffectSettings[] { bloom, grain, chromaticAberration, glitch, grade });
        vol.weight = 0f;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag != "Bullet" || onCooldown) return;
        timerCircle.color = pausedColor;
        onCooldown = true;
        wonderingText.text = messages[Random.Range(0, messages.Count)];
        DOTween.To(() => vol.weight, x => vol.weight = x, 1f, 3f).SetUpdate(true) ;
        DOTween.To(() => Time.timeScale, x => Time.timeScale = x, 0f, .5f).SetUpdate(true).OnComplete(OnTimeSlow);
        uiText.ForEach(t => DOTween.To(() => t.alpha, x => t.alpha = x, 1f, 4f).SetUpdate(true));
        if (Random.Range(0f, 1f) > .95f)
        {
            vid.DOFade(.3f, 4f).SetUpdate(true);
            vid.GetComponent<VideoPlayer>().Play();
        }
        uiSprite.ForEach(t => t.DOFade(1f, 4f).SetUpdate(true));
    }

    private void OnTimeSlow()
    {
        timeSlowed = true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Collision with " + collision.gameObject.tag);
        Debug.Log("Cooldown: " + onCooldown);
        Debug.Log("Time slowed: " + timeSlowed);
        Debug.Log("Invuln Time: " + invulnTimer);
        if (onCooldown && !timeSlowed && invulnTimer <= 0f && Time.timeScale > .99f)
        {
            DestroyPlayer();
            DOTween.To(() => vol.weight, x => vol.weight = x, 1f, 3f).SetUpdate(true);
            DOTween.To(() => Time.timeScale, x => Time.timeScale = x, 0f, .9f).SetUpdate(true);
            DOTween.To(() => deathText.alpha, x => deathText.alpha = x, 1f, 4f).SetUpdate(true);
        }
    }

    private void DestroyPlayer()
    {
        GameObject cyl = Instantiate(DeathCylinder);
        cyl.transform.position = transform.position;
        Destroy(gameObject);
    }

    private void Update()
    {
        if (timeSlowed)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                forceParticles.Play();
                Collider[] bullets = Physics.OverlapSphere(transform.position, 5f).Where(c => c.tag == "Bullet").ToArray();
                foreach (Collider bullet in bullets)
                {
                    Vector3 pos = transform.position;
                    pos.y = bullet.transform.position.y;
                    bullet.GetComponent<Rigidbody>().AddExplosionForce(1200f, pos, 5f);
                }

                EndFreeze();
            }
            else if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.D))
            {
                Debug.Log("Starting dash");
                foreach (GameObject after in afterImages)
                {
                    Renderer aRenderer = after.GetComponent<Renderer>();
                    aRenderer.material.DOFade(.1f, .5f);
                }
                StartCoroutine(Dash());
                EndFreeze();
            }


            if (Input.GetKeyDown(KeyCode.Space))
            {
                EndFreeze();
            }
        }
        if (invulnTimer > 0f)
        {
            invulnTimer -= Time.deltaTime;
            if (invulnTimer <= 0f)
            {
                StopCoroutine(flashingCor);
                renderer.material = NormalMat;
                GetComponent<Collider>().enabled = true;
            }
        }
    }

    private void EndFreeze()
    {
        DOTween.CompleteAll();
        DOTween.To(() => vol.weight, x => vol.weight = x, 0f, 1.5f).SetUpdate(true);
        DOTween.To(() => Time.timeScale, x => Time.timeScale = x, 1f, 1.5f).SetUpdate(true);
        uiText.ForEach(t => DOTween.To(() => t.alpha, x => t.alpha = x, 0f, 1.5f).SetUpdate(true));
        uiSprite.ForEach(t => t.DOFade(0f, 1.5f).SetUpdate(true));
        vid.DOFade(0f, 1.5f).SetUpdate(true);
        vid.GetComponent<VideoPlayer>().Stop();
        timeSlowed = false;
        invulnTimer = invulnTime;
        flashingCor = StartCoroutine(FlashInvuln());
        GetComponent<Collider>().enabled = false;
    }

    private void FixedUpdate()
    {
        if (onCooldown && !timeSlowed)
        {
            cooldownTimer += Time.fixedDeltaTime;
            timerCircle.color = coolDownColor;
            float perc = cooldownTimer / cooldown;
            timerCircle.fillAmount = perc;
            Vector3 rot = timerHand.transform.eulerAngles;
            rot.z = perc * -360f;
            timerHand.transform.eulerAngles = rot;
            if (cooldownTimer > cooldown)
            {
                timerCircle.color = chargedColor;
                onCooldown = false;
                cooldownTimer = 0f;
            }
        }
    }

    IEnumerator FlashInvuln()
    {
        bool norm = true;
        while (true)
        {
            Debug.Log("Flashing invuln");
            if (norm) renderer.material = InvulnFlashMat;
            else renderer.material = NormalMat;
            norm = !norm;
            yield return new WaitForSeconds(0.075f);
        }
    }

    IEnumerator Dash()
    {
        PlayerMovement move = GetComponent<PlayerMovement>();
        float oldMove = move.moveSpeed;
        DOTween.To(() => move.moveSpeed, x => move.moveSpeed = x, dashSpeed, .5f).SetUpdate(true);
        move.moveSpeed = dashSpeed;
        yield return new WaitForSeconds(dashTime);
        move.moveSpeed = oldMove;
        DOTween.To(() => move.moveSpeed, x => move.moveSpeed = x, oldMove, .5f).SetUpdate(true);
        foreach (GameObject after in afterImages)
        {
            Renderer aRenderer = after.GetComponent<Renderer>();
            aRenderer.material.DOFade(0f, .5f);
        }
    }
}
