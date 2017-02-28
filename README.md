# Coding Practices

---

## Logging

---

Une stratégie de log systématique permet de mieux comprendre le
comportement de son programme.

1. Niveaux de logs: `Verbose, Normal, Warning, ...`
2. Configuration de targets: fichiers, buffer,
output, rest, ...
3. Log du cycle de vie des os mobiles (`OnCreate, OnDestroy,` etc...)
4. Log à l'entrée des méthodes `public`
5. Catégoriser les logs

Renvoyer les x dernières lignes de log avec les crash reports ([voir getDescription de HockeyApp](https://support.hockeyapp.net/kb/client-integration-android/customization-options-for-android#-span-id-getdescription-method-getdescription-a-))

---

## Mocks

* Utilisation de mocks systématique pour valider toutes les hypothèses UI
* Injection des dépendances par constructeur

---

## Maîtriser l'état (1/2)

* Notions de design by contract `precondition`, `postcondition`, throw Exception en Debug => tests unitaires
  * Eviter les variables `Nullables`
  * Toujours assigner les listes par défaut (dans constructeur par défaut par exemple)
  * Contrôler la cohérence des données le plus tôt possible

----

## Maîtriser l'état (2/2)

* Les interfaces sont des [modificateurs d'accès](http://blog.ploeh.dk/2011/02/28/Interfacesareaccessmodifiers/)
  * Si une collection ne peut être modifiée utiliser `IReadOnlyList` 
* Créer des Exceptions qui ont du sens (`MappingException`, `CommunicationException`, etc...)

---

## Partage de code

* Trop de centralisation ViewModel = perte de flexibilité UI
  * DiffUtils
* MVVM (NPC, Command) a été pensé pour interagir avec WPF (dual binding)
* Approche pragmatique

---

## View Models (1/3)

Proposition de migration vers un monde post MVVM où la View est maître et subit le
moins possible les callbacks du view model (minimiser/supprimer `INotifyPropertyChanged`).
* Simplification du workflow de la View (unidirectionnel)
* Ajout de la notion d'état UI du view model (`ViewModelState`)

----

## SRP-YAGNI-BLABLA-ETC (2/3)

`CONSTAT:` le view model est trop souvent un god object avec responsabilités multiples<br>
`PROPOSITION:` composition `vs.` héritage

* Sub view models (view models à section de type "Détail")
* Le ViewModel se concentre sur l'affichage des données et délègue le reste à des sous-composants
  * `Paginator` pour les listes infinies
  * `NotifyTask` lancement des tâche async (suppression de async void)

----

## NotifyTask (3/3)

* Adapté des wrappers de Task MVVM de Stephen Cleary
* Callbacks disponibles pour chaque état de la tâche (`IsNotCompleted`, `IsFaulted`, `IsSuccessfullyCompleted`, etc..)
* Relation forte entre `ViewModelState` et états de la tâche
* Gère les exceptions pour nous

---

## Views (Divide and Conquer)

* Ne pas hésiter à découper ses vues en sous-vues
* Mettre en relation sous-vues avec sub view models
* Maitriser le cycle de vie des objets
  * `Contract.Require(() => ViewModel != null)`

---

## Unsubscribe (GC)

`MUST READ` [Xamarin docs garbage_collection]( https://developer.xamarin.com/guides/android/advanced_topics/garbage_collection/)<br>
`MUST SEE` [Advanced Memory Management Evolve 2013]( https://www.youtube.com/watch?v=VJsmrTQWD2k)

* Attention au leak d'activité sur Android
* Problématique du ref count sur iOS

[Memory Perf Best Practices](https://developer.xamarin.com/guides/cross-platform/deployment,_testing,_and_metrics/memory_perf_best_practices/)




