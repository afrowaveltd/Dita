# Ονομαζόμενοι κάτοχοι τοπικοποίησης

Η Dita υποστηρίζει ** με το όνομα placeholders** σε συμβολοσειρές εντοπισμού, επιτρέποντας την εισαγωγή δυναμικών τιμών κατά τη διάρκεια του runtime, ενώ διατηρεί τη σωστή γραμματική σε όλες τις γλώσσες.

## Σύνταξη

Οι τοπικοί κάτοχοι χρησιμοποιούν τη σύνταξη curly-brace μέσα σε τιμές λεξικού JSON:

```json
{
  "WelcomeMessage": "Hello {userName}, you have {count} new messages",
  "DiskStatus": "Disk {diskName} is {status} with {usagePercent}% used"
}
```

Σε αντίθεση με τους κατόχους θέσεων (, ), που ονομάζονται τοπικοί κάτοχοι είναι ** γλώσσα-αγνωστικιστές** - μεταφραστές μπορούν να τους αναδιατάξει για να ταιριάζει γραμματική της γλώσσας-στόχου χωρίς να σπάσει τον κώδικα.

## Αποθήκευση

Οι ονομαζόμενοι τοπικοί κάτοχοι έχουν δύο πηγές αξιών:

### 1. Τιμές runtime (προτείνεται για δυναμικά δεδομένα)

Περάστε τιμές απευθείας κατά την ανάκτηση της εντοπισμένης συμβολοσειράς:

```csharp
// In a Razor page or controller
@inject JsonStringLocalizer Localizer

var message = Localizer["WelcomeMessage", new Dictionary<string, string>
{
    ["userName"] = "John",
    ["count"] = "5"
}];
// Result: "Hello John, you have 5 new messages"
```

### 2. Αποθηκευμένες τιμές (για ημιστατική διαμόρφωση)

Η διαχείριση ενός αρχείου στον κατάλογο:

```json
{
  "WelcomeMessage": {
    "userName": "Guest",
    "count": "0"
  }
}
```

Οι αποθηκευμένες τιμές λειτουργούν ως ** προεπιλεγμένα ** και παρακάμπτονται από τιμές χρόνου λειτουργίας.

## Αναφορά API

### Δείκτης εντοπισμού JsonString

```csharp
// Without placeholders (backward compatible)
LocalizedString text = localizer["SomeKey"];

// With positional formatting (backward compatible)
LocalizedString text = localizer["SomeKey", "arg1", "arg2"];

// With named placeholders (new)
LocalizedString text = localizer["SomeKey", new Dictionary<string, string>
{
    ["name"] = "value"
}];
```

### iplaceholder service υπηρεσία

```csharp
public interface IPlaceholderService
{
    // Get stored placeholders for a key
    Dictionary<string, string> GetPlaceholders(string key);
    
    // Set a stored placeholder value
    void SetPlaceholder(string key, string placeholderName, string value);
    
    // Remove all stored placeholders for a key
    void RemoveKey(string key);
    
    // Format a template with placeholders
    string Format(string template, Dictionary<string, string>? values = null);
    
    // Extract placeholder names from template
    string[] ExtractPlaceholders(string template);
    
    // Check if template contains placeholders
    bool HasPlaceholders(string template);
    
    // Prepare text for translation (mask placeholders)
    (string preparedText, Func<string, string> restore) PrepareForTranslation(string template);
    
    // Persist/load from disk
    Task SaveAsync();
    Task LoadAsync();
}
```

### Μέθοδοι επέκτασης

Για ευκολία όταν εργάζεστε με:

```csharp
public static class StringLocalizerExtensions
{
    public static LocalizedString WithPlaceholders(
        this IStringLocalizer localizer,
        string name,
        Dictionary<string, string> placeholders);
}
```

Χρήση:
```csharp
var text = Localizer.WithPlaceholders("WelcomeMessage", new Dictionary<string, string>
{
    ["userName"] = Model.UserName
});
```

## Μεταφραστική συμπεριφορά

Όταν η υπηρεσία αυτόματης μετάφρασης συναντά κείμενο με τους κατονομαζόμενους τοποθέτες:

1. ** Πριν τη μετάφραση **: Οι τοπικοί κάτοχοι είναι καλυμμένοι με ασφαλή σημεία () για να αποτρέψει τη μεταφραστική μηχανή από την τροποποίηση τους.
2. ** Κατά τη διάρκεια της μετάφρασης **: Ο μεταφραστικός κινητήρας επεξεργάζεται μόνο το μεταφρασμένο κείμενο.
3. **Μετά τη μετάφραση **: Τα αρχικά ονόματα τοποθέτων () αποκαθίστανται στις σωστές θέσεις τους.

### Παράδειγμα

Πηγή (Αγγλικά):

Προπαρασκευασμένα για μετάφραση:

Μεταφράστηκε στα τσεχικά:

Τελικό αποτέλεσμα:

Αυτό εξασφαλίζει ότι:
- Οι τοπικοί κάτοχοι ποτέ δεν μεταφράζονται ή δεν αλλοιώνονται
- Η γραμματική της γλώσσας-στόχου μπορεί να αναδιοργανώσει το περιβάλλον κείμενο ελεύθερα
- Το ίδιο πρότυπο λειτουργεί σωστά σε όλες τις γλώσσες

## Βέλτιστες πρακτικές

1. **Χρήση περιγραφικών ονομάτων **: είναι καλύτερη από ή
2. ** Κρατήστε τους κατόχους θέσεων ελάχιστο **: Πάρα πολλοί τοποθέτες κάνουν τη μετάφραση δυσκολότερη
3. **Έγγραφο αναμένεται τύπους **: Σχόλια στο αρχείο JSON βοήθεια μεταφραστές κατανόηση πλαίσιο
4. ** Προκαθορισμένες τιμές χρόνου εκτέλεσης **: Για πραγματικά δυναμικά δεδομένα (όνομα χρήστη, αριθμοί, ημερομηνίες), τιμές επιτυχίας στο χρόνο εκτέλεσης
5. ** Χρήση αποθηκευμένων τιμών για προεπιλεγμένα **: Για ρυθμίσεις που σπάνια αλλάζουν (όνομα εφαρμογής, email υποστήριξης)
6. ** Διαφορετικοί κάτοχοι**: Χρήση για την επαλήθευση όλων των αναμενόμενων κατόχων

## Ενσωμάτωση με αυτόματη μετάφραση

Η αυτόματη διαχείριση της διατήρησης τοποθέτων κατά τη διάρκεια LibreTranslate κλήσεις. Δεν απαιτείται πρόσθετη ρύθμιση.

Οι και οι δύο χρησιμοποιούν την υπηρεσία επανέναρξης, έτσι ώστε όλες οι μεταφράσεις λεξικών JSON να υποστηρίζουν διαφανώς που ονομάζονται placeholders.

## Συμβατότητα προς τα πίσω

Υφιστάμενος κωδικός που χρησιμοποιεί κατόχους θέσεων ή κανένας κάτοχος θέσης εξακολουθεί να λειτουργεί αμετάβλητος:

```csharp
// Still works exactly as before
var text = localizer["Hello"];
var formatted = localizer["Value is {0}", 42];
```

Το όνομα placeholder API είναι πρόσθετο — δεν σπάει την υπάρχουσα χρήση.
