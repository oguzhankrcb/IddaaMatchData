# IddaaMatchData

### Açıklama

Projenin amacı istenilen bir maçın tüm iddaa oranlarını çekebilmektir, istenirse canlı maçları çekmek vs. içinde kullanılabilir, Web Service içerisinde ki "GetPlayableMatches" fonksiyonunda tüm canlı maçları çeken bir kod bloğu bulunmakta oradan yararlanabilirsiniz. Proje'de bir adet console application (TestApp) ve bir adet Web Service (IddaaSimuService) bulunmakta console projesini çalıştırarak direkt olarak maç iddaa bilgilerini görüntüleyebilirsiniz, maç bilgilerini çektiğinizde aşşağıda ki gibi bir çıktı alırsınız :

![Screenshot_1](https://user-images.githubusercontent.com/7572058/96720026-e874b080-13b2-11eb-99ca-116ff5160af6.png)

## Web Service Kısmı

Web Service ile de istediğiniz bir bağlantı tipini kullanarak (projede entityframework ile mssql server bağlanılarak depolama işlemi yapıldı) maçlara ait iddaa oranlarını alabilirsiniz, halı hazırda bulunan;

![Screenshot_2](https://user-images.githubusercontent.com/7572058/96720291-40131c00-13b3-11eb-806f-4e926a94ca2e.png)

fonksiyonu ile de verilen tarihe ait o saate kadar oynanmamış maçları çektirebilirsiniz.

## Çekilebilen Oranlar

![Screenshot_3](https://user-images.githubusercontent.com/7572058/96720950-21615500-13b4-11eb-9063-3b47481598b2.png)
