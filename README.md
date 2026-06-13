# System Rezerwacji Wizyt "Kliniki-Med"

## Krótki opis projektu

"Kliniki-Med" to klasyczna aplikacja webowa (MPA) stworzona z myślą o ułatwieniu pacjentom intuicyjnego umawiania wizyt u różnych specjalistów na terenie całej Polski. Aplikacja rozwiązuje problem "zmarnowanych" terminów, wprowadzając system wymagalności potwierdzania obecności na wizycie.

Platforma składa się z dwóch głównych modułów:

* **Panelu Pacjenta**: umożliwiającego wyszukiwanie wolnych terminów, rezerwację, odwoływanie wizyt oraz ocenianie lekarzy.


* **Panelu Administratora**: pozwalającego na kompleksowe zarządzanie grafikami, personelem, usługami oraz listą placówek medycznych.



Dzięki wykorzystaniu usług działających w tle (Background Services), aplikacja automatycznie zarządza harmonogramem, zwalnia niepotwierdzone terminy i rozsyła powiadomienia e-mail bez blokowania interfejsu użytkownika.

## Lista użytych technologii

**Backend:**

* **C# / .NET**
* **ASP.NET Core MVC** – główny wzorzec architektoniczny.


* **ASP.NET Core Web API** – do asynchronicznej obsługi żądań z interfejsu użytkownika.


* **ASP.NET Core Identity** – zaawansowany system uwierzytelniania, autoryzacji i zarządzania rolami użytkowników.


* **System.Threading.Channels** – wykorzystane do stworzenia asynchronicznej, nieblokującej kolejki wysyłania wiadomości e-mail.


* **Background Services** (IHostedService) – wątki robocze w tle odpowiedzialne za mailing i automatyzację harmonogramu.



**Baza danych:**

* **Microsoft SQL Server**.


* **Entity Framework Core** – ORM wykorzystany w podejściu Code-First.



**Frontend:**

* **HTML5 / CSS3** – z rygorystycznym odseparowaniem struktury od stylów (brak stylów *inline*).


* **JavaScript (Fetch API / AJAX)** – do dynamicznego, asynchronicznego ładowania danych bez przeładowywania strony.


* **Select2** – biblioteka jQuery do obsługi zaawansowanych, kaskadowych i przeszukiwalnych list rozwijanych.



## Instrukcja uruchomienia (Środowisko lokalne)

Aby uruchomić aplikację na swoim komputerze, postępuj zgodnie z poniższymi krokami:

### Wymagania wstępne

1. Zainstalowane środowisko **Visual Studio 2022** (lub nowsze) / JetBrains Rider / VS Code.
2. Zainstalowany pakiet **.NET SDK** (odpowiedni dla użytej w projekcie wersji).
3. Dostęp do lokalnego serwera bazy danych **Microsoft SQL Server** (np. *SQL Server Express LocalDB* instalowany z Visual Studio).

### Krok po kroku:

**1. Pobranie projektu:**
Sklonuj repozytorium na swój dysk lokalny za pomocą Git lub pobierz projekt jako plik ZIP i wypakuj go.

```bash
git clone https://github.com/Kayan50/App_projekt_IT.git

```

**5. Uruchomienie aplikacji:**
Kliknij zielony przycisk **Uruchom** (lub wciśnij `F5`) w Visual Studio. Przeglądarka otworzy się automatycznie, a aplikacja będzie działać pod adresem `localhost`.

### Dodatkowe informacje

* **Konta użytkowników:** Pierwsza rejestracja w systemie tworzy standardowe konto pacjenta. Aby zalogować się do panelu administracyjnego, konto musi posiadać przypisaną rolę "Admin" w bazie danych. Zmiany tej można dokonać bezpośrednio w tabeli `AspNetUserRoles` po utworzeniu bazy.


*Autorzy: Tomasz Król, Adam Łukasik*
