using UnityEngine;
using System.IO.Ports;
using System.Threading;

public class UcakKontrol : MonoBehaviour
{
    public string portAdi = "COM4";
    private SerialPort stream;
    private Thread portThread;
    private bool threadCalisiyor = false;

    private string sonGelenVeri = "";
    private readonly object veriKilidi = new object();

    private float pitch = 0f;
    private float roll = 0f;
    private float yaw = 0f;

    private float pitchOfset = 0f;
    private float rollOfset = 0f;
    private bool ilkVeriAlindi = false;

    [Header("Sürüş Seti Hız Ayarları")]
    public float mevcutHiz = 50f;
    public float minimumHiz = 20f;
    public float maksimumHiz = 120f;
    public float gazIvmesi = 40f;

    [Header("Dinamikler")]
    public float donusHassasiyeti = 0.6f;
    [Range(1f, 20f)] public float jetAgirligi = 3f;
    [Range(0.01f, 1f)] public float hassasiyet = 0.12f;

    public bool pitchDüzYön = true;
    public bool rollDüzYön = true;

    // --- GÜNCEL SES AYARLARI ---
    private AudioSource motorSesi;
    [Header("Ses Tonu Ayarları (Gaza basınca kükreme)")]
    public float enDusukSesTonu = 0.5f; // Rölantideki motor sesi tonu
    public float enYuksekSesTonu = 1.8f; // Tam gazdaki canavar motor sesi tonu

    void Start()
    {
        // Bileşeni buluyoruz
        motorSesi = GetComponent<AudioSource>();
        if (motorSesi == null)
        {
            motorSesi = GetComponentInChildren<AudioSource>();
        }

        stream = new SerialPort(portAdi, 115200);
        stream.ReadTimeout = 50;

        try
        {
            stream.Open();
            threadCalisiyor = true;
            portThread = new Thread(ArduinoDinle);
            portThread.Start();
            Debug.Log("Sürüş seti arka plan dinlemesi başlatıldı.");
        }
        catch (System.Exception e) { Debug.LogError("Port açılırken hata: " + e.Message); }
    }

    void ArduinoDinle()
    {
        while (threadCalisiyor && stream != null && stream.IsOpen)
        {
            try
            {
                string veri = stream.ReadLine();
                if (!string.IsNullOrEmpty(veri)) { lock (veriKilidi) { sonGelenVeri = veri; } }
            }
            catch (System.Exception) { }
        }
    }

    void Update()
    {
        // İLERİ HAREKET
        transform.Translate(Vector3.forward * mevcutHiz * Time.deltaTime, Space.Self);

        // USB KOL GAZ/FREN
        if (Input.GetKey(KeyCode.JoystickButton5) || Input.GetKey(KeyCode.W))
            mevcutHiz = Mathf.MoveTowards(mevcutHiz, maksimumHiz, gazIvmesi * Time.deltaTime);
        else if (Input.GetKey(KeyCode.JoystickButton4) || Input.GetKey(KeyCode.S))
            mevcutHiz = Mathf.MoveTowards(mevcutHiz, minimumHiz, gazIvmesi * Time.deltaTime);
        else
            mevcutHiz = Mathf.MoveTowards(mevcutHiz, 50f, (gazIvmesi / 2f) * Time.deltaTime);

        // --- SES HIZ KONTROLÜ (GARANTİLİ YÖNTEM) ---
        if (motorSesi != null)
        {
            // Hızı 0 ile 1 arasında bir orana çeviriyoruz
            float hizOrani = (mevcutHiz - minimumHiz) / (maksimumHiz - minimumHiz);
            
            // Sesi bu orana göre inceltip kalınlaştırıyoruz
            motorSesi.pitch = Mathf.Lerp(enDusukSesTonu, enYuksekSesTonu, hizOrani);
        }

        // VERİYİ GÜVENLİCE OKU VE İŞLE
        string işlenecekVeri = "";
        lock (veriKilidi) { işlenecekVeri = sonGelenVeri; }

        if (!string.IsNullOrEmpty(işlenecekVeri) && işlenecekVeri.Contains(","))
        {
            try
            {
                string[] veriler = işlenecekVeri.Split(',');
                if (veriler.Length == 2)
                {
                    float yeniPitch = float.Parse(veriler[0]);
                    float yeniRoll = float.Parse(veriler[1]);

                    if (!ilkVeriAlindi) { pitchOfset = yeniPitch; rollOfset = yeniRoll; ilkVeriAlindi = true; }

                    float netPitch = (yeniPitch - pitchOfset) * hassasiyet;
                    float netRoll = (yeniRoll - rollOfset) * hassasiyet;

                    float pCarpan = pitchDüzYön ? 1f : -1f;
                    float rCarpan = rollDüzYön ? 1f : -1f;

                    pitch = Mathf.Lerp(pitch, netPitch * pCarpan, Time.deltaTime * jetAgirligi);
                    roll = Mathf.Lerp(roll, netRoll * rCarpan, Time.deltaTime * jetAgirligi);

                    yaw += roll * donusHassasiyeti * Time.deltaTime;
                    transform.localRotation = Quaternion.Euler(pitch, yaw, -roll);
                }
            }
            catch (System.Exception) { }
        }
    }

    void OnApplicationQuit()
    {
        threadCalisiyor = false;
        if (portThread != null && portThread.IsAlive) portThread.Abort();
        if (stream != null && stream.IsOpen) stream.Close();
    }
}