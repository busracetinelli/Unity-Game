using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class stacks : MonoBehaviour
{
    int stack_uzunlugu;
    int count = 10;
    int skorsayısı = 0;
    GameObject[] go_stack;//(1)her bir stack nesnesini kaydetmek için dizi tanımlıyorum.
    int stack_index;//(2)oyunun devam edebilmesi için en altta bulunan stack'imizi bir üste getireceğiz. Bunu yapabilmek önce bir değişken tanımlamam gerekiyor.
    bool stack_alındı=false;//(3) Üste koyulan stack'in X ekseninin farklı bir konumdan gelmesini ayarlamak için bir değişken tanımlıyorum.
    const float max_deger = 7f;//(3) Üste alınan stack'in gidebileceği maksimum X konumunu belirlemek için değeri değişkene tanımlıyorum.
    const float hiz_degeri = 0.15f;//(3) Üste aldığımız stack'in hareketli bir şekilde gelmesini istediğimiz için hareket hızını tanımlıyorum.
    float hiz = hiz_degeri;//(3)
    bool x_ekseninde_hareket;
    const float buyukluk = 4f; //(4) Üste getirilen stacklarımıza dokunduğumuzda boşlukta kalan kısmının parçalanması işlemini yapacağız. Öncelikleri staklarımınzın büyüklük değerini tanımlıyorum
    Vector2 stack_boyut = new Vector2(buyukluk, buyukluk);//(4) Stacklerimiz parçalanınca yeni gelen stacklerinde boyutunu küçültmek için yeni bir vector tanımlıyorum.
    Vector3 Camera_pos;//kamera takibini yapmak için bir vector tanımlıyorum.
    Vector3 eski_stack_pos;//(5) Karşılşatırma yapmak için vector tanımlıyorum.
    float hassasiyet;
    bool dead = false;
    float hatapayı=0.2f;
    int combo = 0;

    Color32 renk;
    public Color32 renk1;
    public Color32 renk2;
    public Color32 renk3;
    public Color32 renk4;
    public Text skor;
    public GameObject go_panel;
    public Text highscore_text;
    int sayac = 0;
    Camera camera;

    int highscore;


    // Start is called before the first frame update
    void Start()
    {
        highscore = PlayerPrefs.GetInt("En Yüksek Skor:");
        highscore_text.text = highscore.ToString();
        skor.text = skorsayısı.ToString();
        camera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        camera.backgroundColor = renk2;
        renk = renk1;
        stack_uzunlugu = transform.childCount;//kaç adet stack uzunluğu olduğunu belirliyorum
        go_stack = new GameObject[stack_uzunlugu];
        //(1)var olan tüm stack'lerimizi(child'ı) diziye aktardım
        for (int i = 0; i < stack_uzunlugu; i++)
        {
            go_stack[i] = transform.GetChild(i).gameObject;//(1)bu kod sayesinde bulunan tüm küpleri diziye aktardım
            go_stack[i].GetComponent<Renderer>().material.color = Color32.Lerp(go_stack[stack_index].GetComponent<Renderer>().material.color, renk, 0.3f); ;
        }
        stack_index = stack_uzunlugu - 1;//(2)belirlediğimiz index sayesinden en  alttaki bulunan stack'i bir üste koyacağım.
    }
    
    void ArtıkParcaOl(Vector3 konum,Vector3 scale,Color32 renkparca)//(6) Boşluk kısmında kalan stacklerimizin aşağıya düşmesini sağlıyorum.
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.transform.localScale = scale;
        go.transform.position = konum;
        go.GetComponent<Renderer>().material.color = renkparca;
        go.AddComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!dead)
        {
            if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer)
            {
                if (Input.GetMouseButtonDown(0))//Mouse'un sol tıkına her tıklandığında belirtilen işlemleri yapacak.
                {
                    Oyun();

                }
                Hareket();
                transform.position = Vector3.Lerp(transform.position, Camera_pos, 0.1f);
            }
            else if(Application.platform == RuntimePlatform.Android)
            {
                if(Input.touchCount>00 && Input.GetTouch(0).phase == TouchPhase.Began)
                {
                    Oyun();
                }
                Hareket();
                transform.position = Vector3.Lerp(transform.position, Camera_pos, 0.1f);//LERP komutu sayesinde kameramızın yumuşak geçiş yapmasını sağlıyorum.
            }
            
        }
    }

    public void Oyun()
    {
        if (Stack_Kontrol())
        {
            Stack_Al_Koy();
            count++;
            skorsayısı++;
            skor.text = skorsayısı.ToString();
            if (skorsayısı > highscore)
            {
                highscore = skorsayısı;
            }
            byte deger = 25;
            renk = new Color32((byte)(renk.r + deger), (byte)(renk.g + deger), (byte)(renk.b + deger), renk.a);
            renk2 = new Color32((byte)(renk2.r + deger), (byte)(renk2.g + deger), (byte)(renk2.b + deger), renk2.a);
            if (sayac > 5)
            {
                sayac = 0;
                renk1 = renk2;
                renk2 = renk3;
                renk3 = renk4;
                renk4 = renk;
                renk = renk1;

            }
            sayac++;
        }
        else
        {
            Bitir();
        }
    }
    void Stack_Al_Koy()//(2) en altta bulunan stack'i üste koyabilmemiz için bir void tanımlayıp işlemleri yapıyoruz.
    {
        eski_stack_pos = go_stack[stack_index].transform.localPosition;
        if (stack_index <= 0)//(2) hata vermemesi için başa döndürmemiz gerekiyor.Yoksa gameobject'te 0 dan başladığımız için -1. item yok diyecektir.
        {
            stack_index = stack_uzunlugu;
        }
        stack_alındı = false;
        stack_index--;//(2) stack_index'i her seferinde 1 azalttığımız için her seferinde en alttakini bir üste yerleştirecek.
        x_ekseninde_hareket = !x_ekseninde_hareket;
        Camera_pos = new Vector3(0, -count+8, 0);//kameramızın hangi eksende takip edeceğini belirtiyoruz.
        go_stack[stack_index].transform.localScale = new Vector3(stack_boyut.x, 1, stack_boyut.y);// Üste gelecek olan yeni stack'in bir altında olan stack ile boyutlarının aynı olmasını sağladım.
        go_stack[stack_index].GetComponent<Renderer>().material.color = Color32.Lerp(go_stack[stack_index].GetComponent<Renderer>().material.color, renk, 0.5f);
        //Camera camera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        camera.backgroundColor = Color32.Lerp(camera.backgroundColor, renk2, 0.1f);


    }

    void Hareket()//(3) Var olan Stacklerimizin hareket etmesini sağlıyoruz.
    {
        if (x_ekseninde_hareket)
        {
            if (!stack_alındı)
            {
                go_stack[stack_index].transform.localPosition = new Vector3(-2, count, hassasiyet);//(3)Üste çıkardığımız değerin X ekseninde -2 konumundan gelmesini ayarlıyorum.
                stack_alındı = true;
            }
            if (go_stack[stack_index].transform.localPosition.x > max_deger)
            {
                hiz = hiz_degeri * -1;
            }
            else if (go_stack[stack_index].transform.localPosition.x < -max_deger)
            {
                hiz = hiz_degeri;
            }
            go_stack[stack_index].transform.localPosition += new Vector3(hiz, 0, 0);
        }
        else
        {
            if (!stack_alındı)
            {
                go_stack[stack_index].transform.localPosition = new Vector3(hassasiyet,count,-2);//(3)Üste çıkardığımız değerin Z ekseninde -2 konumundan gelmesini ayarlıyorum.
                stack_alındı = true;
            }
            if (go_stack[stack_index].transform.localPosition.z > max_deger)
            {
                hiz = hiz_degeri * -1;
            }
            else if (go_stack[stack_index].transform.localPosition.z < -max_deger)
            {
                hiz = hiz_degeri;
            }
            go_stack[stack_index].transform.localPosition += new Vector3(0, 0, hiz);
        }
    }

    public  bool Stack_Kontrol()//(5) Stacklerimiz dışarda kalıp kalmadğını anlamak için stacklarimizi karşılaştırıcam.
    {
        if (x_ekseninde_hareket)
        {
            float fark = eski_stack_pos.x - go_stack[stack_index].transform.localPosition.x;
            if (Mathf.Abs(fark) > hatapayı)
            {
                combo = 0;
                Vector3 konum;
                if (go_stack[stack_index].transform.localPosition.x > eski_stack_pos.x)
                {
                    konum = new Vector3(go_stack[stack_index].transform.position.x + go_stack[stack_index].transform.localScale.x / 2, go_stack[stack_index].transform.position.y, go_stack[stack_index].transform.position.z);
                }
                else
                {
                    konum = new Vector3(go_stack[stack_index].transform.position.x - go_stack[stack_index].transform.localScale.x / 2, go_stack[stack_index].transform.position.y, go_stack[stack_index].transform.position.z);
                }
                Vector3 boyut = new Vector3(fark, 1, stack_boyut.y);
                stack_boyut.x -= Mathf.Abs(fark);
                if (stack_boyut.x < 0)
                {
                    return false;
                }
                go_stack[stack_index].transform.localScale = new Vector3(stack_boyut.x, 1, stack_boyut.y);
                float mid = go_stack[stack_index].transform.localPosition.x / 2 + eski_stack_pos.x / 2;
                go_stack[stack_index].transform.localPosition = new Vector3(mid, count, eski_stack_pos.z);
                hassasiyet = go_stack[stack_index].transform.localPosition.x;
                ArtıkParcaOl(konum, boyut, go_stack[stack_index].GetComponent<Renderer>().material.color);
            }
            else
            {
                combo++;
                if (combo > 3)
                {
                    stack_boyut.x += 0.3f;
                    if (stack_boyut.x > buyukluk)
                    {
                        stack_boyut.x = buyukluk;
                    }
                    go_stack[stack_index].transform.localScale = new Vector3(stack_boyut.x, 1, stack_boyut.y);                  
                    go_stack[stack_index].transform.localPosition = new Vector3(eski_stack_pos.x, count, eski_stack_pos.z);
                }
                else
                {
                    go_stack[stack_index].transform.localPosition = new Vector3(eski_stack_pos.x, count, eski_stack_pos.z);
                }
                hassasiyet = go_stack[stack_index].transform.localPosition.x;

            }
        }
        else
        {
            float fark = eski_stack_pos.z - go_stack[stack_index].transform.localPosition.z;
            if (Mathf.Abs(fark) > hatapayı)
            {
                combo = 0;
                Vector3 konum;
                if (go_stack[stack_index].transform.localPosition.z > eski_stack_pos.z)
                {
                    konum = new Vector3(go_stack[stack_index].transform.position.x, go_stack[stack_index].transform.position.y, go_stack[stack_index].transform.position.z + go_stack[stack_index].transform.localScale.z / 2);
                }
                else
                {
                     konum = new Vector3(go_stack[stack_index].transform.position.x, go_stack[stack_index].transform.position.y, go_stack[stack_index].transform.position.z - go_stack[stack_index].transform.localScale.z / 2);
                }
                Vector3 boyut = new Vector3(stack_boyut.x, 1, fark);

                stack_boyut.y -= Mathf.Abs(fark);
                if (stack_boyut.y < 0)
                {
                     return false;
                }
                 go_stack[stack_index].transform.localScale = new Vector3(stack_boyut.x, 1, stack_boyut.y);
                float mid = go_stack[stack_index].transform.localPosition.z / 2 + eski_stack_pos.z / 2;
                go_stack[stack_index].transform.localPosition = new Vector3(eski_stack_pos.x, count, mid);
                hassasiyet = go_stack[stack_index].transform.localPosition.z;
                ArtıkParcaOl(konum, boyut, go_stack[stack_index].GetComponent<Renderer>().material.color);
                combo++;
            }
            else
            {
                combo++;
                if (combo >=1 )
                {
                    stack_boyut.y += 0.3f;
                    if (stack_boyut.y > buyukluk)
                    {
                        stack_boyut.y = buyukluk;                       
                    }
                    go_stack[stack_index].transform.localScale = new Vector3(stack_boyut.x, 1, stack_boyut.y);
                    go_stack[stack_index].transform.localPosition = new Vector3(eski_stack_pos.x, count, eski_stack_pos.z);
                }
                else
                {
                    go_stack[stack_index].transform.localPosition = new Vector3(eski_stack_pos.x, count, eski_stack_pos.z);
                }
                hassasiyet = go_stack[stack_index].transform.localPosition.z;

            }
        }   
        return true;
        
    }
    void Bitir()
    {
        dead = true;
        go_stack[stack_index].AddComponent<Rigidbody>();
        go_panel.SetActive(true);
        PlayerPrefs.SetInt("En Yüksek Skor:", highscore);
        highscore_text.text = highscore.ToString();
        skor.text = "";
    }
    public void Yeni_Oyun()
    {
        Application.LoadLevel(Application.loadedLevel);
    }
}
