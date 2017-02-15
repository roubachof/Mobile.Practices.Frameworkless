## SeLoger.Lab.Playground.Droid
Best practices for Xamarin developpement post MVVM (without binding) (Xamarin.Android and iOS)

### Logging

Une stratégie de log systématique permet de mieux comprendre le
comportement de son programme.

1. Niveaux de logs: `Verbose, Normal, Warning, ...`
2. Configuration de targets: fichiers, buffer,
output, rest, ...
3. Log du cycle de vie des os mobiles (`OnCreate, OnDestroy,` etc...)
4. Log à l'entrée des méthodes `public`
5. Catégoriser les logs

Renvoyer les x dernières lignes de log avec les crash reports ([voir getDescription de HockeyApp](https://support.hockeyapp.net/kb/client-integration-android/customization-options-for-android#-span-id-getdescription-method-getdescription-a-))

#### Candidats sur mobile

* Metrolog
* Serilog

### Service rest

Nouvelle équipe côté mobile développant un `Adapter` mobile agrégeant les 4 WS
Seloger.

`+` Abstraction du merdier <br>
`+` Unique interlocuteur <br>
`+` Optimisation transmission des données (usage de protobuf) <br>

`-` Temps de calcul intermédiaire

#### Candidats

Service Stack

### Mocks

* Utilisation de mocks systématique pour valider toutes les hypothèses UI
* Injection des dépendances par constructeur

### Base de données

Passage en no sql ? <br>
Migration toujours nécessaire.

* [Couchbase Lite](https://developer.couchbase.com/documentation/mobile/1.3/guides/couchbase-lite/index.html) (ForestDB ou SQLite)
* [LiteDB](http://www.litedb.org/)
* [MarcelloDB](http://www.marcellodb.org/)

### View Models

Proposition de migration vers un monde post MVVM où la View est maître et subit le
moins possible les callbacks du view model (minimiser `INotifyPropertyChanged`).
* Simplification du workflow de la View (unidirectionnel)
* Ajout de la notion d'état du view model (`enum Loading/SuccessfullyLoaded/CommunicationError/etc...`)

### SRP-YAGNI-DIVIDE AND CONQUER-ETC

`CONSTAT:` le view model base est trop souvent un god object avec responsabilités multiples<br>
`PROPOSITION:` composition `vs.` héritage

* Sub view models (view models à section de type "Détail")
* Le ViewModel se concentre sur l'affichage des données et délègue le reste à des sous-composants
  * `Paginator` pour les listes infinies
  * `NotifyTask` pour chargement des données et état du chargement

### Formulaires

* Stop au dual binding à gogo
* L'encapsulation du model ne doit pas être automatique
  * Si `model` est très différent des champs du `viewmodel`  préférer une stratégie de copie
  * Idem si il n'y a pas de relation `1..1` (par exemple 2 champs se combinent en 1)
  * Cas ou propriété ViewModel peut être null alors que celle du Model est requise

### Exemple

```csharp
public string CombinedField
    get $"{Model.Prop1}/{Model.Prop2}"
    set
        var splittedField = value.Split('/')
        Model.Prop1 = splittedField[0]
        Model.Prop2 = splittedField[1]
```
```csharp
public async Task Load()
    var model = await FetchModel()
    CombinedField = $"{Model.Prop1}/{Model.Prop2}"

public async Task Save()
    var model = new FormModel()
    var splittedField = value.Split('/')
    model.Prop1 = splittedField[0]
    model.Prop2 = splittedField[1]
    await SaveModel(model)
```

### Views (Divide and Conquer)

* Ne pas hésiter à découper ses vues en sous-vues
* Mettre en relation sous-vues avec sub view models
* Maitriser le cycle de vie des objets
  * `Contract.Require(ViewModel != null)`

### Unsubscribe (GC)

`MUST READ` [Xamarin docs garbage_collection]( https://developer.xamarin.com/guides/android/advanced_topics/garbage_collection/)<br>
`MUST SEE` [Advanced Memory Management Evolve 2013]( https://www.youtube.com/watch?v=VJsmrTQWD2k)

A cause des joyeusetés de Xamarin (GC cross ref)
* Weak events (`ViewModelLoaded`)
* Weak references
* Unsubscribe / Set handler, propriétés = null

[Memory Perf Best Practices](https://developer.xamarin.com/guides/cross-platform/deployment,_testing,_and_metrics/memory_perf_best_practices/)

### Maîtriser l'état (1/2)

* Notions de design by contract `precondition`, `postconditions`, throw Exception en Debug => tests unitaires
  * Eviter les variables `Nullables`
  * Toujours assigner les listes par défaut (dans constructeur par défaut par exemple)
  * Contrôler la cohérence des données le plus tôt possible

### Maîtriser l'état (2/2)

* Notions fonctionnelles 
  * Objets immuables ? (multithread)
  * Entités (objets avec id) => Comparaison par Id et non par référence
* Créer des Exceptions qui ont du sens (`MappingException`, `CommunicationException`, etc...)
