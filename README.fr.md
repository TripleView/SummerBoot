[Documents en anglais](https://github.com/TripleView/SummerBoot/blob/master/README.md)\|[Document chinois](https://github.com/TripleView/SummerBoot/blob/master/README.zh-cn.md)

[![ GitHub license ](https://img.shields.io/badge/license-MIT-blue.svg)](https://raw.githubusercontent.com/dotnetcore/CAP/master/LICENSE.txt)

# Merci pour la licence ide fournie par jetbrain

<a href="https://jb.gg/OpenSourceSupport"><img src="https://resources.jetbrains.com/storage/products/company/brand/logos/jb_beam.png?_ga=2.140768178.1037783001.1644161957-503565267.1643800664&_gl=1*1rs8z57*_ga*NTAzNTY1MjY3LjE2NDM4MDA2NjQ.*_ga_V0XZL7QHEB*MTY0NDE2MTk1Ny4zLjEuMTY0NDE2NTE2Mi4w" width = "200" height = "200" alt="" align=center /></a>

# SummerBoot (nom chinois : Summer Boot)

Afin de permettre à chacun de mieux comprendre l'utilisation de summerBoot , j'ai créé un exemple de projet-[SummerBootAdmin](https://github.com/TripleView/SummerBootAdmin), un cadre général de gestion du back-end basé sur la séparation du front-end et du back-end, vous pouvez consulter le code de ce projet pour mieux comprendre comment utiliser summerBoot .

# Idée centrale

Combinez les concepts avancés de SpringBoot avec la simplicité et l'élégance de C#, la programmation déclarative, concentrez-vous sur "ce qu'il faut faire" plutôt que sur "comment le faire", et écrivez du code à un niveau supérieur. SummerBoot s'engage à créer un -cadre humanisé d'utilisation et facile à entretenir, afin que chacun puisse quitter le travail plus tôt pour faire ce qu'il aime.

# Description du cadre

Il s'agit d'un framework entièrement déclaratif qui implémente divers appels sous la forme d'annotations + interfaces, y compris, mais sans s'y limiter, la base de données, http, le cache, etc. Le framework générera automatiquement des classes d'implémentation d'interface via la technologie Reflection Emit.

# Rejoignez le groupe QQ pour des commentaires et des suggestions

Numéro de groupe : 799648362

# Commencer

## Nuget

SummerBoot dans votre projet .

`PM> Install-Package SummerBoot `

# cadre de support

net core 3.1, net 6

# répertoire de documents

-   [Merci pour la licence ide fournie par jetbrain](#thanks-for-the-ide-license-provided-by-jetbrain)
-   [SummerBoot (nom chinois : Summer Boot)](#summerboot-中文名夏日启动)
-   [Idée centrale](#core-idea)
-   [Description du cadre](#framework-description)
-   [Rejoignez le groupe QQ pour des commentaires et des suggestions](#join-the-qq-group-for-feedback-and-suggestions)
-   [Commencer](#getting-started)
    -   [Nuget](#nuget)
-   [cadre de support](#support-frame)
-   [répertoire de documents](#document-directory)
-   [SummerBoot utilise un référentiel pour les opérations de base de données](#summerboot-uses-repository-for-database-operations)
    -   [Préparation](#preparation)
    -   [1.Service d'enregistrement](#1registration-service)
    -   [2.Définir une classe d'entité de base de données](#2define-a-database-entity-class)
    -   [3. Écrivez un contrôleur pour générer automatiquement des tables de base de données via des classes d'entités](#3write-a-controller-to-automatically-generate-database-tables-through-entity-classes)
    -   [4.Définir l'interface de stockage](#4define-storage-interface)
    -   [5.Ajouter, supprimer, modifier et interroger, tous prennent en charge la synchronisation asynchrone](#5add-delete-modify-and-query-all-support-asynchronous-synchronization)
        -   [5.1 Ajouté](#51-added)
            -   [5.1.1 L'interface a sa propre méthode Insert, qui peut insérer une seule entité ou une liste d'entités](#511-the-interface-has-its-own-insert-method-which-can-insert-a-single-entity-or-a-list-of-entities)
            -   [5.1.2 Insertion rapide des lots, l'interface de stockage est livrée avec la méthode FastBatchInsert, qui peut insérer rapidement la liste des entités.](#512-fast-batch-insertion-the-storage-interface-comes-with-the-fastbatchinsert-method-which-can-quickly-insert-the-entity-list)
        -   [5.2 supprimer](#52-delete)
            -   [5.2.1 L'interface est livrée avec une méthode Delete, qui peut supprimer une seule entité ou une liste d'entités](#521-the-interface-comes-with-a-delete-method-which-can-delete-a-single-entity-or-a-list-of-entities)
            -   [5.2.2 Prend également en charge la suppression basée sur des expressions lambda, renvoyant le nombre de lignes affectées, par exemple](#522-also-supports-deletion-based-on-lambda-expressions-returning-the-number-of-affected-rows-for-example)
        -   [mise à jour 5.3](#53-update)
            -   [5.3.1 L'interface est livrée avec une méthode Update, qui peut mettre à jour une seule entité ou une liste d'entités](#531-the-interface-comes-with-an-update-method-which-can-update-a-single-entity-or-a-list-of-entities)
            -   [5.3.2 Il prend également en charge la méthode de mise à jour basée sur la syntaxe de la chaîne Lambda](#532-it-also-supports-the-update-method-based-on-lambda-chain-syntax)
        -   [5.4 Requête](#54-query)
            -   [5.4.1 Requête de syntaxe de chaîne Lambda, telle que :](#541-lambda-chain-syntax-query-such-as)
            -   [5.4.2 Définir des méthodes directement dans l'interface et ajouter des annotations aux méthodes, telles que Sélectionner, Mettre à jour, Supprimer](#542-define-methods-directly-in-the-interface-and-add-annotations-to-the-methods-such-as-select-update-delete)
            -   [5.4.4 Les annotations de sélection sont fusionnées de cette manière là où les conditions de requête](#544-select-annotations-are-spliced-in-this-way-where-query-conditions)
        -   [5.5 Prise en charge des transactions](#55-transaction-support)
        -   [5.6 Classes d'implémentation personnalisées dans des cas particuliers](#56-custom-implementation-classes-in-special-cases)
            -   [5.6.1 Définir une interface héritée de IBaseRepository et définir vos propres méthodes dans l'interface](#561-define-an-interface-inherited-from-ibaserepository--and-define-your-own-methods-in-the-interface)
            -   [5.6.2 Ajoutez une classe d'implémentation, héritée de la classe CustomBaseRepository et de l'interface personnalisée ICustomCustomerRepository, et ajoutez l'annotation AutoRegister à la classe d'implémentation.](#562-add-an-implementation-class-inherited-from-the-custombaserepository-class-and-the-custom-icustomcustomerrepository-interface-and-add-the-autoregister-annotation-to-the-implementation-class)
            -   [5.6.3 Exemple d'utilisation](#563-example-of-use)
    -   [6 Générez automatiquement des classes d'entités basées sur des tables de base de données ou générez automatiquement des instructions ddl pour des tables de base de données basées sur des classes d'entités](#6-automatically-generate-entity-classes-based-on-database-tables-or-automatically-generate-ddl-statements-for-database-tables-based-on-entity-classes)
        -   [6.1 Espace de noms de table](#61-table-namespace)
        -   [6.2 Générer automatiquement l'instruction ddl de la table de la base de données en fonction de la classe d'entité](#62-automatically-generate-the-ddl-statement-of-the-database-table-according-to-the-entity-class)
        -   [6.2.2 Mappage de type ou mappage de nom des champs de classe d'entité personnalisée aux champs de base de données](#622-type-mapping-or-name-mapping-from-custom-entity-class-fields-to-database-fields)
        -   [6.3 Générer automatiquement des classes d'entités basées sur des tables de base de données](#63-automatically-generate-entity-classes-based-on-database-tables)
-   [SummerBoot utilise feinte pour passer des appels http](#summerboot-uses-feign-to-make-http-calls)
    -   [1.Service d'enregistrement](#1registration-service-1)
    -   [2.Définir l'interface](#2define-the-interface)
    -   [3.Définissez l'en-tête de la demande (en-tête)](#3set-the-request-header-header)
    -   [4. Intercepteur personnalisé](#4custom-interceptor)
    -   [5.Définir la méthode](#5define-the-method)
        -   [5.1 Paramètres communs à la méthode](#51-common-parameters-in-the-method)
        -   [5.2 Paramètres spéciaux dans la méthode](#52-special-parameters-in-the-method)
            -   [5.2.1 Les paramètres ajoutent des annotations de requête](#521-parameters-add-query-annotations)
                -   [5.2.1.1 L'annotation Query est utilisée avec l'annotation Embedded, et la classe d'annotation Embedded peut être ajoutée en tant que paramètre dans son ensemble](#5211-the-query-annotation-is-used-with-the-embedded-annotation-and-the-embedded-annotation-class-can-be-added-as-a-parameter-as-a-whole)
            -   [5.2.2 Les paramètres ajoutent des annotations Body (BodySerializationKind.Form)](#522-parameters-add-body-bodyserializationkindform-annotations)
            -   [5.2.3 Les paramètres ajoutent des annotations Body (BodySerializationKind.Json)](#523-parameters-add-body-bodyserializationkindjson-annotations)
            -   [5.2.4 Utiliser la classe spéciale HeaderCollection comme paramètre de méthode pour ajouter des en-têtes de requête par lots](#524-use-the-special-class-headercollection-as-a-method-parameter-to-add-request-headers-in-batches)
            -   [5.2.5 Utilisez la classe spéciale BasicAuthorization comme paramètre de méthode pour ajouter l'en-tête de demande d'autorisation pour l'authentification de base](#525-use-the-special-class-basicauthorization-as-a-method-parameter-to-add-the-authorization-request-header-for-basic-authentication)
            -   [5.2.6 Utilisez la classe spéciale MultipartItem comme paramètre de méthode et marquez l'annotation Multipart sur la méthode pour télécharger la pièce jointe](#526-use-the-special-class-multipartitem-as-a-method-parameter-and-mark-the-multipart-annotation-on-the-method-to-upload-the-attachment)
            -   [5.2.7 Utilisez la classe Stream comme type de retour de la méthode pour recevoir des données en continu, telles que le téléchargement de fichiers.](#527-use-the-class-stream-as-the-return-type-of-the-method-to-receive-streaming-data-such-as-downloading-files)
            -   [5.2.8 Utilisez la classe HttpResponseMessage comme type de retour de la méthode pour obtenir le message de réponse le plus original.](#528-use-the-class-httpresponsemessage-as-the-return-type-of-the-method-to-get-the-most-original-response-message)
            -   [5.2.9 Utilisez la classe Task comme type de retour de la méthode, c'est-à-dire qu'aucune valeur de retour n'est requise.](#529-use-the-class-task-as-the-return-type-of-the-method-that-is-no-return-value-is-required)
    -   [6.Microservice - accès aux nacos](#6microservice---access-to-nacos)
        -   [6.1 Ajouter la configuration nacos dans le fichier de configuration](#61-add-nacos-configuration-in-the-configuration-file)
        -   [6.2 Accéder au centre de configuration nacos](#62-access-nacos-configuration-center)
        -   [6.3 Accéder au centre de service nacos](#63-access-nacos-service-center)
            -   [6.3.1 Ajouter une configuration dans StartUp.cs](#631-add-configuration-in-startupcs)
            -   [6.3.2 Définir l'interface d'appel des microservices](#632-define-the-interface-for-calling-microservices)
    -   [7.Utilisation des cookies en contexte](#7using-cookies-in-context)
-   [SummerBoot utilise le cache pour les opérations de cache](#summerboot-uses-cache-for-cache-operations)
    -   [1.Service d'enregistrement](#1registration-service-2)
    -   [2. Interface ICache](#2icache-interface)
    -   [3.Après avoir injecté l'interface, elle peut être utilisée](#3after-injecting-the-interface-it-can-be-used)

# SummerBoot utilise un référentiel pour les opérations de base de données

summerBoot a développé son propre module ORM basé sur l'unité de travail et le mode d'entreposage, c'est-à-dire le référentiel, qui prend en charge plusieurs bases de données et plusieurs liens, y compris les opérations d'ajout, de suppression, de modification et d'interrogation de cinq types de bases de données courants (sqlserver, mysql, oracle, sqlite, pgsql) , s'il existe d'autres exigences de base de données, vous pouvez vous référer aux 5 codes sources ci-dessus et contribuer aux codes de ce projet. orm ne prend pas en charge les requêtes lambda pour les requêtes conjointes multi-tables, car je pense que c'est plus facile à utiliser et gérer les requêtes multi-tables directement en SQL.

## Préparation

Vous devez installer le package de dépendance de base de données correspondant via nuget , tel que Microsoft.Data.SqlClient de SqlServer , Mysql.data de mysql , Oracle.ManagedDataAccess.Core d'oracle, Npgsql de pgsql

## 1.Service d'enregistrement

Le référentiel prend en charge plusieurs bases de données et plusieurs liens. Dans le référentiel, nous appelons un lien une unité de base de données. L'exemple suivant illustre l'ajout de deux unités de base de données à un projet. La première est le type de base de données mysql et la seconde est le type de base de données sqlserver . , ajoutez une unité de base de données via la méthode AddDatabaseUnit, les paramètres sont la classe dbConnection de la base de données correspondante et l'interface d'unité de travail (l'interface d'unité de travail est utilisée pour les transactions), le framework fournit 9 interfaces d'unité de travail IUnitOfWork1~IUnitOfWork9 par défaut, bien sûr, vous pouvez également personnaliser l'interface de l'unité de travail, il vous suffit d'hériter de l'interface de IUnitOfWork. Parce qu'il existe plusieurs unités de base de données, l'entrepôt doit être lié à l'unité de base de données correspondante. Vous pouvez lier un seul entrepôt via BindRepository , ou ajoutez une annotation personnalisée sur l'entrepôt (cette annotation doit être héritée de AutoRepositoryAttribute , qui est fourni par le framework par défaut.AutoRepository1Attribute~AutoRepository9Attribute), puis utilisez la méthode BindRepositorysWithAttribute pour lier les entrepôts par lots. En même temps, vous pouvez ajouter des rappels de pré-insertion et des rappels de pré-mise à jour dans l'unité (par exemple, il peut être utilisé pour ajouter l'heure de création et l'heure de mise à jour), ajouter des mappages de types personnalisés et ajouter des mappages de champs personnalisés Gestionnaire, ajouter le nom de la table mappage, ajouter le mappage de nom de champ (le mappage de nom de table et le mappage de nom de champ peuvent être utilisés dans des situations où les champs de base de données et les champs de classe d'entité sont différents, comme la base de données oracle, les noms de champ de nom de table sont tous en majuscules, mais les classes d'entité sont nommées Pascal), etc.

```csharp
builder.Services.AddSummerBoot();

builder.Services.AddSummerBootRepository(it =>
{
		var mysqlDbConnectionString = builder.Configuration.GetValue<string>("mysqlDbConnectionString");
		//Add the first mysql type database unit
		it.AddDatabaseUnit<MySqlConnection, IUnitOfWork1>(mysqlDbConnectionString,
				x =>
				{
						//Add field name mapping
						//x.ColumnNameMapping = (columnName) =>
						//{
						//    return columnName.ToUpper();
						//};

						//Add table name mapping
						//x.TableNameMapping = (tableName) =>
						//{
						//    return tableName.ToUpper();
						//};

						//Bind a single Repository
						//x.BindRepository<IMysqlCustomerRepository,Customer>();

						//Batch binding Repositorys through custom annotations
						x.BindRepositorysWithAttribute<AutoRepository1Attribute>();

						 //Bind database generation interface
						x.BindDbGeneratorType<IDbGenerator1>();

						 //Callback before binding insert
						x.BeforeInsert += entity =>
						{
								if (entity is BaseEntity baseEntity)
								{
										baseEntity.CreateOn = DateTime.Now;
								}
						};

						 //Callback before binding update
						x.BeforeUpdate += entity =>
						{
								if (entity is BaseEntity baseEntity)
								{
										baseEntity.LastUpdateOn = DateTime.Now;
								}
						};
						
						//Add custom type mapping
						//x.SetParameterTypeMap(typeof(DateTime), DbType.DateTime2);

						//Add custom field mapping handler
						//x.SetTypeHandler(typeof(Guid), new GuidTypeHandler());

				});

		//Add a second database unit of type sqlserver
		var sqlServerDbConnectionString = builder.Configuration.GetValue<string>("sqlServerDbConnectionString");
		it.AddDatabaseUnit<SqlConnection, IUnitOfWork2>(sqlServerDbConnectionString,
				x =>
				{
						x.BindRepositorysWithAttribute<AutoRepository2Attribute>();
						x.BindDbGeneratorType<IDbGenerator2>();
				});
});
```

## 2.Définir une classe d'entité de base de données

La plupart des annotations de classe d'entité proviennent de l'espace de noms système System.ComponentModel.DataAnnotations et System.ComponentModel.DataAnnotations.Schema , comme le nom de la table Table, le nom de la colonne Column, la clé primaire Key, la clé primaire auto-incrémentée DatabaseGenerated (DatabaseGeneratedOption.Identity) , nom de colonne Colonne, Ce champ NotMapped n'est pas mappé et Description est utilisé pour générer des annotations lorsque les classes d'entités génèrent des tables de base de données. En même temps, certaines annotations sont personnalisées, telles que l'ignorance de la colonne IgnoreWhenUpdateAttribute lors de la mise à jour (principalement utilisée pour les champs qui n'ont pas besoin d'être mis à jour lors de la mise à jour), définissons une classe d'entité Customer

```csharp
[Description("Member")]
public class Customer
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { set; get; }
    ///<summary>
    /// Last update time
    ///</summary>
    [Description("Last update time")]
    public DateTime? LastUpdateOn { get; set; }
    ///<summary>
    /// Last updated by
    ///</summary>
    [Description("Last Updater")]
    public string LastUpdateBy { get; set; }
    ///<summary>
    /// Creation time
    ///</summary>
    [IgnoreWhenUpdate]
    [Description("Creation Time")]
    public DateTime? CreateOn { get; set; }
    ///<summary>
    /// founder
    ///</summary>
    [IgnoreWhenUpdate]
    [Description("Creator")]
    public string CreateBy { get; set; }
    ///<summary>
    /// is it effective
    ///</summary>
    [Description("Is it valid")]
    public int? Active { get; set; }
    [Description("Name")]
    public string Name { set; get; }
    [Description("age")]
    public int Age { set; get; }

    [Description("Membership Number")]
    public string CustomerNo { set; get; }

    [Description("total consumption amount")]
    public decimal TotalConsumptionAmount { set; get; }
}
```

SummerBoot est livré avec une classe d'entité de base BaseEntity (oracle est OracleBaseEntity). La classe d'entité comprend cinq champs : identifiant à augmentation automatique, créateur, heure de création, mise à jour, heure de mise à jour et validité. Il est recommandé que la classe d'entité directement inherit BaseEntity , alors le client ci-dessus peut être abrégé comme suit :

```csharp
[Description("Member")]
public class Customer : BaseEntity
{
    [Description("Name")]
    public string Name { set; get; }
    [Description("age")]
    public int Age { set; get; }

    [Description("Membership Number")]
    public string CustomerNo { set; get; }

    [Description("total consumption amount")]
    public decimal TotalConsumptionAmount { set; get; }
}
```

## 3. Écrivez un contrôleur pour générer automatiquement des tables de base de données via des classes d'entités

```csharp
[ApiController]
[Route("[controller]/[action]")]
public class GenerateTableController : Controller
{
    private readonly IDbGenerator1 dbGenerator1;

    public GenerateTableController(IDbGenerator1 dbGenerator1)
    {
        this.dbGenerator1 = dbGenerator1;
    }

    [HttpGet]
    public IActionResult Index()
    {
        var results = dbGenerator1.GenerateSql(new List<Type>() { typeof(Customer) });
        foreach (var result in results)
        {
            dbGenerator1.ExecuteGenerateSql(result);
        }
        return Content("ok");
    }
}
```

Accédez à l'interface d'index et le framework générera automatiquement une table Customer avec des annotations.

## 4.Définir l'interface de stockage

L'interface de stockage doit être héritée de l'interface IBaseRepository, et l'interface est annotée avec AutoRepository1 pour l'enregistrement automatique.

```csharp
[AutoRepository1]
public interface ICustomerRepository : IBaseRepository<Customer>
{
}
```

## 5.Ajouter, supprimer, modifier et interroger, tous prennent en charge la synchronisation asynchrone

### 5.1 Ajouté

#### 5.1.1 L'interface a sa propre méthode Insert, qui peut insérer une seule entité ou une liste d'entités

Si le nom de la clé primaire de la classe d'entité est Id, et qu'il y a une annotation Key, et qu'elle augmente automatiquement, comme suit :

```csharp
[Key, DatabaseGenerated (DatabaseGeneratedOption.Identity)]
public int Id { set ; get; }
```

Ensuite, après l'insertion, le framework attribuera automatiquement une valeur au champ ID de l'entité, qui est une valeur d'ID auto-incrémentée.

```csharp
[ApiController]
[Route("[controller]/[action]")]
public class CustomerController : Controller
{
    private readonly ICustomerRepository customerRepository;

    public CustomerController(ICustomerRepository customerRepository)
    {
        this.customerRepository = customerRepository; _
    }

    [HttpGet]
    public async Task<IActionResult> Insert()
    {
        var customer = new Customer() { Name = "testCustomer" };
        await customerRepository.InsertAsync(customer);

        var customer2 = new Customer() { Name = "testCustomer2" };
        var customer3 = new Customer() { Name = "testCustomer3" };
        var customerList = new List<Customer>() { customer2, customer3 };
        await customerRepository.InsertAsync(customerList);

        return Content("ok");
    }
}
```

#### 5.1.2 Insertion rapide des lots, l'interface de stockage est livrée avec la méthode FastBatchInsert, qui peut insérer rapidement la liste des entités.

Dans le cas d'une insertion rapide par lots, le framework n'attribuera pas automatiquement de valeur au champ ID de l'entité. En même temps, si la base de données est mysql , il existe des circonstances particulières. Tout d'abord, la bibliothèque de pilotes doit avoir MySqlConnector . Cette bibliothèque peut coexister avec mysql.data et ne sera pas en conflit, il n'y a donc pas lieu de s'inquiéter, et la chaîne de connexion à la base de données doit être suivie de "; AllowLoadLocalInfile =true", et en même temps exécuter "set global local_infile =1" sur la base de données mysql pour activer le téléchargement par lots. SQLite ne prend pas en charge l'insertion rapide par lots.

```csharp
var customer2 = new Customer() { Name = "testCustomer2" };
var customer3 = new Customer() { Name = "testCustomer3" };
var customerList = new List<Customer>() { customer2, customer3 };
customerRepository.FastBatchInsert(customerList);
```

### 5.2 supprimer

#### 5.2.1 L'interface est livrée avec une méthode Delete, qui peut supprimer une seule entité ou une liste d'entités

```csharp
customerRepository.Delete(customer);

customerRepository.Delete(customerList);
```

#### 5.2.2 Prend également en charge la suppression basée sur des expressions lambda, renvoyant le nombre de lignes affectées, par exemple

```csharp
var deleteCount = customerRepository.Delete(it => it.Age > 5);
```

### mise à jour 5.3

#### 5.3.1 L'interface est livrée avec une méthode Update, qui peut mettre à jour une seule entité ou une liste d'entités

Mettre à jour en fonction de la clé primaire. Si la clé primaire est combinée, des annotations de clé peuvent être ajoutées à plusieurs champs.

```csharp
customerRepository.Update(customer);

customerRepository.Update(customerList);
```

#### 5.3.2 Il prend également en charge la méthode de mise à jour basée sur la syntaxe de la chaîne Lambda

Le sql de mise à jour généré ne contient que les champs définis et renvoie le nombre de lignes affectées, par exemple

```csharp
var updateCount = customerRepository.Where(it => it.Name == "testCustomer")
    .SetValue(it => it.Age, 5)
    .SetValue(it => it.TotalConsumptionAmount, 100)
    .ExecuteUpdate();
```

### 5.4 Requête

Il prend en charge les requêtes normales et les requêtes paginées, et il existe deux façons d'interroger.

#### 5.4.1 Requête de syntaxe de chaîne Lambda, telle que :

```csharp
//regular query
var allCustomers = await customerRepository.GetAllAsync();

var customerById = await customerRepository.GetAsync(1);

var customers = await customerRepository.Where(it => it.Age > 5).ToListAsync();

var maxTotalConsumptionAmount = await customerRepository.MaxAsync(it => it.TotalConsumptionAmount);

var names = new List<string>() { "testCustomer", "testCustomer3" };
var customers2 = await customerRepository.Where(it => names.Contains(it.Name)).ToListAsync();

var firstItem = await customerRepository.FirstOrDefaultAsync(it => it.Name == "testCustomer");
// pagination
var pageResult = await customerRepository.Where(it => it.Age > 5).Skip(0).Take(10).ToPageAsync();

var pageable = new Pageable(1, 10);
var pageResult2 = await customerRepository.ToPageAsync(pageable);
```

#### 5.4.2 Définir des méthodes directement dans l'interface et ajouter des annotations aux méthodes, telles que Sélectionner, Mettre à jour, Supprimer

Ensuite, écrivez des instructions sql dans Select, Update, Delete , telles que

```csharp
[AutoRepository1]
public interface ICustomerRepository : IBaseRepository<Customer>
{
    //async
    [Select("select od.productName from customer c join orderHeader oh on c.id= oh.customerid" +
            "join orderDetail od on oh.id= od.OrderHeaderId where c.name=@name")]
    Task<List<CustomerBuyProduct>> QueryAllBuyProductByNameAsync(string name);

    [Select("select * from customer where age>@age order by id")]
    Task<Page<Customer>> GetCustomerByPageAsync(IPageable pageable, int age);

    //sync
    [Select("select od.productName from customer c join orderHeader oh on c.id= oh.customerid" +
            "join orderDetail od on oh.id= od.OrderHeaderId where c.name=@name")]
    List<CustomerBuyProduct> QueryAllBuyProductByName(string name);

    [Select("select * from customer where age>@age order by id")]
    Page<Customer> GetCustomerByPage(IPageable pageable, int age);

}
```

Instructions:

```csharp
var result = await customerRepository.QueryAllBuyProductByNameAsync("testCustomer");

//page
var pageable = new Pageable(1, 10);
var page = customerRepository.GetCustomerByPage(pageable, 5);
```

> Remarque : 5.4.2 Prise en charge de la pagination dans la requête, la valeur de retour de la méthode est enveloppée par la classe Page et le paramètre method doit inclure le paramètre de pagination IPageable , et l'instruction sql doit également avoir order by, par exemple :
>
> ```csharp
> [Select("select * from customer where age>@age order by id")]
> Page<Customer> GetCustomerByPage(IPageable pageable, int age);
> ```

Le sql dans l'annotation prend en charge la lecture à partir de la configuration
Le json configuré est le suivant :

```json
{
    "mysqlSql": {
        "QueryListSql":"select * from customer",
        "QueryByPageSql":"select * from customer order by age",
        "UpdateByNameSql":"update customer set age=@age where name=@name",
        "DeleteByNameSql":"delete from customer where name=@name"
    }
}
```

Les éléments de configuration sont enveloppés par ${} et l'interface est la suivante :

```csharp
[AutoRepository]
public interface ICustomerTestConfigurationRepository : IBaseRepository<Customer>
{
    //asynchronous
    [Select("${ mysqlSql:QueryListSql }")]
    Task<List<Customer>> QueryListAsync();

    [Select("${ mysqlSql:QueryByPageSql }")]
    Task<Page<Customer>> QueryByPageAsync(IPageable pageable);
    //asynchronous
    [Update("${ mysqlSql:UpdateByNameSql }")]
    Task<int> UpdateByNameAsync(string name, int age);

    [Delete("${ mysqlSql:DeleteByNameSql }")]
    Task<int> DeleteByNameAsync(string name);

    //Synchronize
    [Select("${ mysqlSql:QueryListSql }")]
    List<Customer> QueryList();

    [Select("${ mysqlSql:QueryByPageSql }")]
    Page<Customer> QueryByPage(IPageable pageable);
    //asynchronous
    [Update("${ mysqlSql:UpdateByNameSql }")]
    int UpdateByName(string name, int age);

    [Delete("${ mysqlSql:DeleteByNameSql }")]
    int DeleteByName(string name);
}
```

#### 5.4.4 Les annotations de sélection sont fusionnées de cette manière là où les conditions de requête

Enveloppez une seule condition de requête avec {{}}, et une seule variable peut être incluse dans une condition. En même temps, lors de la définition d'une méthode, le paramètre est défini comme WhereItem \\&lt;T>, et T est un paramètre générique, indiquant le type de paramètre réel. De cette manière, summerboot traitera automatiquement les conditions de la requête. Les règles de traitement sont les suivantes. Si l'actif de whereItem est vrai, la condition est activée et les conditions de la requête enveloppé dans {{ }} dans l'instruction sql se développera et participera à la requête. Si active est fausse, le sql La condition de requête enveloppée dans {{ }} dans l'instruction est automatiquement remplacée par une chaîne vide et ne participe pas à query.Afin de rendre whereItem plus utile, la méthode WhereBuilder est fournie.L'exemple d'utilisation est le suivant :

```csharp
//definition
[AutoRepository]
public interface ICustomerRepository : IBaseRepository<Customer>
{
    [Select("select * from customer where 1=1 {{ and name = @name}}{{ and age = @age}}")]
    Task<List<CustomerBuyProduct>> GetCustomerByConditionAsync(WhereItem<string> name, WhereItem<int> age);

    [Select("select * from customer where 1=1 {{ and name = @name}}{{ and age = @age}} order by id")]
    Task<Page<Customer>> GetCustomerByPageByConditionAsync(IPageable pageable, WhereItem<string> name, WhereItem<int> age);
}

//use
var nameEmpty = WhereBuilder.Empty<string>(); //var nameEmpty = new WhereItem<string>(false,"");

var ageEmpty = WhereBuilder.Empty<int>();
var nameWhereItem = WhereBuilder.HasValue("page5"); // var nameWhereItem = WhereItem<string>(true, "page5");
var ageWhereItem = WhereBuilder.HasValue(5);
var pageable = new Pageable(1, 10);

var bindResult = customerRepository.GetCustomerByCondition(nameWhereItem, ageEmpty);
Assert.Single(bindResult);
var bindResult2 = customerRepository.GetCustomerByCondition(nameEmpty, ageEmpty);
Assert.Equal(102, bindResult2.Count);
var bindResult5 = customerRepository.GetCustomerByPageByCondition(pageable, nameWhereItem, ageEmpty);
Assert.Single(bindResult5.Data);
var bindResult6 = customerRepository.GetCustomerByPageByCondition(pageable, nameEmpty, ageEmpty);
```

### 5.5 Prise en charge des transactions

Utilisez l'interface d'unité de travail IUnitOfWork pour implémenter les transactions de base de données. Lors de l'injection de l'interface de stockage personnalisée, elle injecte également l'interface IUnitOfWork correspondant à l'unité de base de données. Ici, il s'agit de IUnitOfWork1. L'utilisation est la suivante

```csharp
[ApiController]
[Route("[controller]/[action]")]
public class CustomerController : Controller
{
    private readonly ICustomerRepository customerRepository;
    private readonly IUnitOfWork1 unitOfWork1;

    public CustomerController(ICustomerRepository customerRepository, IUnitOfWork1 unitOfWork1)
    {
        this.customerRepository = customerRepository; _
        this.unitOfWork1 = unitOfWork1;
    }

    [HttpGet]
    public async Task<IActionResult> UnitOfWorkTest()
    {
        try
        {
            // start transaction
            unitOfWork1.BeginTransaction();
            var customer = new Customer() { Name = "testCustomer" };
            await customerRepository.InsertAsync(customer);

            var customer2 = new Customer() { Name = "testCustomer2" };
            var customer3 = new Customer() { Name = "testCustomer3" };
            var customerList = new List<Customer>() { customer2, customer3 };
            await customerRepository.InsertAsync(customerList);
            await customerRepository.DeleteAsync(it => it.Age == 0);
            //commit transaction
            unitOfWork1.Commit();
        }
        catch (Exception e)
        {
            // rollback transaction
            unitOfWork1.RollBack();
            throw;
        }

        return Content("ok");
    }
}
```

### 5.6 Classes d'implémentation personnalisées dans des cas particuliers

#### 5.6.1 Définir une interface héritée de IBaseRepository et définir vos propres méthodes dans l'interface

> Notez qu'il n'est pas nécessaire d'ajouter des annotations AutoRepository à cette interface pour le moment
>
> ```csharp
> public interface ICustomCustomerRepository : IBaseRepository<Customer>
> {
>     Task<List<Customer>> GetCustomersAsync(string name);
> ```

    Task<Customer> GetCustomerAsync(string name);

    Task<int> UpdateCustomerNameAsync(string oldName, string newName);

    Task<int> CustomQueryAsync();

}

    #### 5.6.2 Add an implementation class, inherited from the CustomBaseRepository class and the custom ICustomCustomerRepository interface, and add the AutoRegister annotation to the implementation class.
    the AutoRegister annotation are the type of the custom interface ICustomCustomerRepository and the declaration cycle ServiceLifetime of the service (the cycle defaults to the scope level).The purpose of adding the AutoRegister annotation is to allow the framework to automatically register the custom interface and the corresponding custom class in the IOC container.It can be directly injected and used.CustomBaseRepository comes with Execute, QueryFirstOrDefault and QueryList and other methods.If you want to contact the lower-level dbConnection for query, refer to the CustomQueryAsync method below.First, OpenDb () opens the database connection, and then query.The query must contain On the database unit information this.databaseUnit and transaction:dbTransaction these two parameters, CloseDb () closes the database connection after the query is completed;

    ```` csharp
    [AutoRegister(typeof(ICustomCustomerRepository))]
    public class CustomCustomerRepository : CustomBaseRepository<Customer>, ICustomCustomerRepository
    {
        public CustomCustomerRepository(IUnitOfWork1 uow) : base(uow, uow.DbFactory)
        {
        }

        public async Task<Customer> GetCustomerAsync(string name)
        {
            var result =
                await this.QueryFirstOrDefaultAsync<Customer>("select * from customer where name=@name", new { name });
            return result;
        }

        public async Task<List<Customer>> GetCustomersAsync(string name)
        {
            var result = await this.QueryListAsync<Customer>("select * from customer where name=@name", new { name });

            return result;
        }

        public async Task<int> UpdateCustomerNameAsync(string oldName, string newName)
        {
            var result = await this.ExecuteAsync("update customer set name=@newName where name=@oldName", new { newName, oldName });
            return result;
        }

        public async Task<int> CustomQueryAsync() _
        {
            this.OpenDb();
            var grid = await this.dbConnection.QueryMultipleAsync(this.databaseUnit, "select id from customer", transaction: dbTransaction);
            var ids = grid.Read<int>().ToList();
            this.CloseDb();
            return ids[0];
        }
    }

#### 5.6.3 Exemple d'utilisation

```csharp
[ApiController]
[Route("[controller]/[action]")]
public class CustomerController : Controller
{
    private readonly ICustomerRepository customerRepository;
    private readonly IUnitOfWork1 unitOfWork1;
    private readonly ICustomCustomerRepository customCustomerRepository;

    public CustomerController(ICustomerRepository customerRepository, IUnitOfWork1 unitOfWork1, ICustomCustomerRepository customCustomerRepository)
    {
        this.customerRepository = customerRepository; _
        this.unitOfWork1 = unitOfWork1;
        this.customCustomerRepository = customCustomerRepository; _
    }

    [HttpGet]
    public async Task<IActionResult> CustomClass()
    {
        try
        {
            // start transaction
            unitOfWork1.BeginTransaction();
            var customer = new Customer() { Name = "testCustomer" };
            await customerRepository.InsertAsync(customer);

            var customer2 = new Customer() { Name = "testCustomer2" };
            var customer3 = new Customer() { Name = "testCustomer3" };
            var customerList = new List<Customer>() { customer2, customer3 };
            await customerRepository.InsertAsync(customerList);

            var result1 = await customCustomerRepository.GetCustomerAsync("testCustomer");
            var result2 = await customCustomerRepository.CustomQueryAsync();

            var result3 = await customCustomerRepository.UpdateCustomerNameAsync("testCustomer3", "testCustomer33");
            var result4 = await customCustomerRepository.GetCustomersAsync("testCustomer");
            //commit transaction
            unitOfWork1.Commit();
        }
        catch (Exception e)
        {
            // rollback transaction
            unitOfWork1.RollBack();
            throw;
        }

        return Content("ok");
    }
}
```

## 6 Générez automatiquement des classes d'entités basées sur des tables de base de données ou générez automatiquement des instructions ddl pour des tables de base de données basées sur des classes d'entités

### 6.1 Espace de noms de table

Les espaces de noms dans sqlserver sont des schémas, les espaces de noms dans oracle sont des schémas et les espaces de noms dans sqlite et mysql sont des bases de données.
Si vous souhaitez définir des tables sous différents espaces de noms, ajoutez simplement[Table("ClientAvecSchéma", Schéma="test")]annotation.

```csharp
[Table("CustomerWithSchema", Schema = "test")]
public class CustomerWithSchema
{
    public string Name { set; get; }
    public int Age { set; get; }
}
```

### 6.2 Générer automatiquement l'instruction ddl de la table de la base de données en fonction de la classe d'entité

Pour l'utilisation, veuillez vous référer aux 3 exemples précédents. Ici, mysql est pris comme exemple. Le sql généré est le suivant :

```sql
CREATE TABLE testSummerboot.`Customer` (
    `Name` text NULL,
    `Age` int NOT NULL,
    `CustomerNo` text NULL,
    `TotalConsumptionAmount` decimal(18,2) NOT NULL,
    `Id` int NOT NULL AUTO_INCREMENT,
    `LastUpdateOn` datetime NULL,
    `LastUpdateBy` text NULL,
    `CreateOn` datetime NULL,
    `CreateBy` text NULL,
    `Active` int NULL,
    PRIMARY KEY (`Id`)
)

ALTER TABLE testSummerboot.`Customer` COMMENT = 'Member'
ALTER TABLE testSummerboot.`Customer` MODIFY `Name` text NULL COMMENT 'Name'
ALTER TABLE testSummerboot.`Customer` MODIFY `Age` int NOT NULL COMMENT 'age'
ALTER TABLE testSummerboot.`Customer` MODIFY `CustomerNo` text NULL COMMENT 'member number'
ALTER TABLE testSummerboot.`Customer` MODIFY `TotalConsumptionAmount` decimal(18,2) NOT NULL COMMENT 'total consumption amount'
ALTER TABLE testSummerboot.`Customer` MODIFY `Id` int NOT NULL AUTO_INCREMENT COMMENT 'Id'
ALTER TABLE testSummerboot.`Customer` MODIFY `LastUpdateOn` datetime NULL COMMENT 'last update time'
ALTER TABLE testSummerboot.`Customer` MODIFY `LastUpdateBy` text NULL COMMENT 'last updated by'
ALTER TABLE testSummerboot.`Customer` MODIFY `CreateOn` datetime NULL COMMENT 'Creation time'
ALTER TABLE testSummerboot.`Customer` MODIFY `CreateBy` text NULL COMMENT 'Created by'
ALTER TABLE testSummerboot.`Customer` MODIFY `Active` int NULL COMMENT 'Valid or not'
```

Le sql généré est sql pour ajouter de nouveaux champs ou sql pour mettre à jour les commentaires. Afin d'éviter la perte de données, il n'y aura pas de sql pour supprimer des champs. Il est plus pratique pour une utilisation quotidienne de diviser l'opération de génération de sql et d'exécution de sql en deux pièces.Vous pouvez rapidement exécuter le sql et le vérifier.Après avoir confirmé qu'il n'y a pas de problème, vous pouvez l'enregistrer et le laisser au dba pour examen lorsque l'application est officiellement publiée

### 6.2.2 Mappage de type ou mappage de nom des champs de classe d'entité personnalisée aux champs de base de données

L'annotation de colonne est uniformément utilisée ici, telle que[Column("Âge",TypeName="float")]

```csharp
[Description("Member")]
public class Customer : BaseEntity
{
    [Description("Name")]
    public string Name { set; get; }
    [Description("age")]
    [Column("Age", TypeName = "float")]
    public int Age { set; get; }

    [Description("Membership Number")]
    public string CustomerNo { set; get; }

    [Description("total consumption amount")]
    public decimal TotalConsumptionAmount { set; get; }
}
```

Le sql généré est le suivant

```sql
CREATE TABLE testSummerboot.`Customer` (
    `Name` text NULL,
    `Age` float NOT NULL,
    `CustomerNo` text NULL,
    `TotalConsumptionAmount` decimal(18,2) NOT NULL,
    `Id` int NOT NULL AUTO_INCREMENT,
    `LastUpdateOn` datetime NULL,
    `LastUpdateBy` text NULL,
    `CreateOn` datetime NULL,
    `CreateBy` text NULL,
    `Active` int NULL,
    PRIMARY KEY (`Id`)
)

```

### 6.3 Générer automatiquement des classes d'entités basées sur des tables de base de données

Injectez l'unité de base de données correspondant à l'interface IDbGenerator, voici l'interface IDbGenerator1, appelez la méthode GenerateCsharpClass pour générer le texte de la classe c#, les paramètres sont la collection de noms de table de base de données et l'espace de noms de la classe d'entité générée, le code est comme suit

```csharp
[ApiController]
[Route("[controller]/[action]")]
public class GenerateTableController : Controller
{
    private readonly IDbGenerator1 dbGenerator1;

    public GenerateTableController(IDbGenerator1 dbGenerator1)
    {
        this.dbGenerator1 = dbGenerator1;
    }

    [HttpGet]
    public async Task<IActionResult> GenerateClass()
    {
        var generateClasses = dbGenerator1.GenerateCsharpClass(new List<string>() { "Customer" }, "Test.Model");
        return Content("ok");
    }

    [HttpGet]
    public IActionResult Index()
    {
        var results = dbGenerator1.GenerateSql(new List<Type>() { typeof(Customer) });
        foreach (var result in results)
        {
            dbGenerator1.ExecuteGenerateSql(result);
        }
        return Content("ok");
    }
}
```

Appelez l'interface GenerateClass, la classe d'entité c# générée est la suivante, créez simplement un nouveau fichier de classe et collez-y le texte

```csharp
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Test.Model
{
    ///<summary>
    ///member
    ///</summary>
    [Table("Customer")]
    public class Customer
    {
        ///<summary>
        ///Name
        ///</summary>
        [Column("Name")]
        public string Name { get; set; }
        ///<summary>
        ///age
        ///</summary>
        [Column("Age")]
        public int Age { get; set; }
        ///<summary>
        ///Member ID
        ///</summary>
        [Column("CustomerNo")]
        public string CustomerNo { get; set; }
        ///<summary>
        ///total consumption amount
        ///</summary>
        [Column("TotalConsumptionAmount")]
        public decimal TotalConsumptionAmount { get; set; }
        ///<summary>
        ///Id
        ///</summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("Id")]
        public int Id { get; set; }
        ///<summary>
        ///Last update time
        ///</summary>
        [Column("LastUpdateOn")]
        public DateTime? LastUpdateOn { get; set; }
        ///<summary>
        ///Last updated by
        ///</summary>
        [Column("LastUpdateBy")]
        public string LastUpdateBy { get; set; }
        ///<summary>
        ///Creation time
        ///</summary>
        [Column("CreateOn")]
        public DateTime? CreateOn { get; set; }
        ///<summary>
        ///founder
        ///</summary>
        [Column("CreateBy")]
        public string CreateBy { get; set; }
        ///<summary>
        ///is it effective
        ///</summary>
        [Column("Active")]
        public int? Active { get; set; }
    }
}

```

# SummerBoot utilise feinte pour passer des appels http

> La couche inférieure de feign est basée sur httpClient .

## 1.Service d'enregistrement

```csharp
services.AddSummerBoot();
services.AddSummerBootFeign();
```

## 2.Définir l'interface

Définissez une interface et ajoutez l'annotation FeignClient à l'interface. Dans l'annotation FeignClient, vous pouvez personnaliser la partie publique de l'url de l'interface http - url (l'url demandée par l'ensemble de l'interface est composée de l'url dans FeignClient plus le chemin dans le méthode), s'il faut ignorer la vérification du certificat https de l'interface distante - IsIgnoreHttpsCertificateValidate , délai d'expiration de l'interface - Timeout (unité s), intercepteur personnalisé - InterceptorType .

```csharp
[FeignClient(Url = "http://localhost:5001/home", IsIgnoreHttpsCertificateValidate = true, InterceptorType = typeof(MyRequestInterceptor), Timeout = 100)]
public interface ITestFeign
{
    [GetMapping("/query")]
    Task<Test> TestQuery([Query] Test tt);
}
```

Dans le même temps, l'url et le chemin peuvent être obtenus en lisant la configuration, et les éléments de configuration sont enveloppés par ${}. Le json de la configuration est le suivant :

```json
{
    "configurationTest": {
        "url":"http://localhost:5001/home",
        "path":"/query"
    }
}
```

L'interface est la suivante :

```csharp
[FeignClient(Url = "${ configurationTest:url }")]
public interface ITestFeignWithConfiguration
{
    [GetMapping("${ configurationTest:path }")]
    Task<Test> TestQuery([Query] Test tt);
}
```

Parfois, nous voulons uniquement utiliser le chemin dans la méthode comme une URL complète pour lancer une requête http, nous pouvons alors définir l'interface comme suit, définir UsePathAsUrl sur true (la valeur par défaut est false)

```csharp
[FeignClient(Url = "http://localhost:5001/home")]
public interface ITestFeign
{
    [PostMapping("http://localhost:5001/home/ json", UsePathAsUrl = true)]
    Task TestUsePathAsUrl([Body(BodySerializationKind.Json)] Test tt);
}
```

## 3.Définissez l'en-tête de la demande (en-tête)

Vous pouvez choisir d'ajouter une annotation Headers sur l'interface, ce qui signifie que toutes les requêtes http sous cette interface porteront l'en-tête de requête dans l'annotation. Le paramètre Headers est un paramètre de type chaîne de longueur variable. En même temps, Headers peut également être ajouté à la méthode, ce qui signifie que lorsque la méthode est appelée, l'en-tête de la requête sera ajouté. Le paramètre Headers de l'interface peut être superposé avec le paramètre Headers de la méthode. En même temps, des variables peuvent être utilisées dans les en-têtes, et les espaces réservés pour les variables sont {{}}, tels que

```csharp
[FeignClient(Url = "http://localhost:5001/home", IsIgnoreHttpsCertificateValidate = true, InterceptorType = typeof(MyRequestInterceptor), Timeout = 100)]
[Headers("a:a", "b:b")]
public interface ITestFeign
{
    [GetMapping("/ testGet")]
    Task<Test> TestAsync();

    [GetMapping("/ testGetWithHeaders")]
    [Headers("c:c")]
    Task<Test> TestWithHeadersAsync();

    //header replacement
    [Headers("a:{ {methodName}}")]
    [PostMapping("/ abc")]
    Task<Test> TestHeaderAsync(string methodName);
}

await TestFeign.TestAsync()
    >>> get, http: //localhost:5001/home/testGet, headers are"a:a"and"b:b"

await TestFeign.TestWithHeadersAsync()
    >>> get, http: //localhost:5001/home/testGetWithHeaders, headers are"a:a","b:b"and"c:c"

await TestFeign.TestHeaderAsync("abc");
    >>> post, http: //localhost:5001/home/abc, and the request header is"a:abc"
```

## 4. Intercepteur personnalisé

Les intercepteurs personnalisés sont efficaces pour toutes les méthodes sous l'interface. Le scénario d'application de l'intercepteur consiste principalement à effectuer certaines opérations avant la demande. Par exemple, avant de demander une interface commerciale tierce, vous devez vous connecter au système tiers. d'abord, vous pouvez ensuite l'utiliser dans l'intercepteur Demandez d'abord l'interface de connexion tierce, après avoir obtenu les informations d'identification, mettez-les dans l'en-tête, l'intercepteur doit implémenter l'interface IRequestInterceptor, l'exemple est le suivant

```csharp
//loginFeign client for login
[FeignClient(Url = "http://localhost:5001/login", IsIgnoreHttpsCertificateValidate = true, Timeout = 100)]
public interface ILoginFeign
{
    [PostMapping("/login")]
    Task<LoginResultDto> LoginAsync([Body()] LoginDto loginDto);
}

//Then customize the login interceptor
public class LoginInterceptor : IRequestInterceptor
{

    private readonly ILoginFeign loginFeign;
    private readonly IConfiguration configuration;

    public LoginInterceptor(ILoginFeign loginFeign, IConfiguration configuration)
    {
        this.loginFeign = loginFeign; _
        this.configuration = configuration;
    }


    public async Task ApplyAsync(RequestTemplate requestTemplate)
    {
        var username = configuration.GetSection("username").Value;
        var password = configuration.GetSection("password").Value;

        var loginResultDto = await this.loginFeign.LoginAsync(new _ LoginDto(){ Name = username, Password = password});
        if (loginResultDto! = null)
        {
            requestTemplate.Headers.Add("Authorization", new List<string>() { "Bearer" + loginResultDto.Token });
        }

        await Task.CompletedTask;
    }
}

//testFegn client that accesses the business interface , and define the interceptor on the client as loginInterceptor
[FeignClient(Url = "http://localhost:5001/home", IsIgnoreHttpsCertificateValidate = true, InterceptorType = typeof(LoginInterceptor), Timeout = 100)]
public interface ITestFeign
{
    [GetMapping("/ testGet")]
    Task<Test> TestAsync();
}

await TestFeign.TestAsync();
>>> get to http: //localhost:5001/home/testGet, header is"Authorization:Bearer abc"

```

IgnoreInterceptor à la méthode , la requête initiée par cette méthode ignorera l'intercepteur, tel que

```csharp
//testFegn client that accesses the business interface , and define the interceptor on the client as loginInterceptor
[FeignClient(Url = "http://localhost:5001/home", IsIgnoreHttpsCertificateValidate = true, InterceptorType = typeof(LoginInterceptor), Timeout = 100)]
public interface ITestFeign
{
    [GetMapping("/ testGet")]
    [IgnoreInterceptor]
    Task<Test> TestAsync();
}

await TestFeign.TestAsync ();
>>> get to http: //localhost:5001/home/testGet, no header

```

Vous pouvez également ajouter un intercepteur global. Lors de l'enregistrement de AddSummerBootFeign , la méthode d'appel est la suivante :

```csharp
services.AddSummerBootFeign(it =>
{
    it.SetGlobalInterceptor(typeof(GlobalInterceptor));
});
```

## 5.Définir la méthode

Chaque méthode doit ajouter des annotations pour représenter le type de requête et l'url à laquelle accéder. Il y a 4 annotations intégrées, GetMapping , PostMapping , PutMapping , DeleteMapping , et la valeur de retour de la méthode doit être de type Task&lt;>

```csharp
[FeignClient(Url = "http://localhost:5001/home", IsIgnoreHttpsCertificateValidate = true, InterceptorType = typeof(MyRequestInterceptor), Timeout = 100)]
public interface ITestFeign
{
    [GetMapping("/ testGet")]
    Task<Test> TestAsync();

    [PostMapping("/ testPost")]
    Task<Test> TestPostAsync();

    [PutMapping("/ testPut")]
    Task<Test> TestPutAsync();

    [DeleteMapping("/ testDelete")]
    Task<Test> TestDeleteAsync();
}
```

### 5.1 Paramètres communs à la méthode

Si le paramètre n'a pas d'annotation spéciale, ou n'est pas une classe spéciale, il sera utilisé comme paramètre dynamique pour participer au remplacement des variables dans l'url et le header (si le paramètre est une classe, lire la valeur de l'attribut de la classe ), et les variables dans l'url et l'en-tête utilisent l'espace réservé { {}}, si le nom de la variable est incohérent avec le nom du paramètre, vous pouvez utiliser l'annotation AliasAs (qui peut être utilisée sur les paramètres ou les attributs de classe) pour spécifier un alias, tel que

```csharp
[FeignClient(Url = "http://localhost:5001/home", IsIgnoreHttpsCertificateValidate = true, InterceptorType = typeof(MyRequestInterceptor), Timeout = 100)]
public interface ITestFeign
{
    // url replacement
    [PostMapping("/{{ methodName }}")]
    Task<Test> TestAsync(string methodName);

    //header replacement
    [Headers("a:{ {methodName}}")]
    [PostMapping("/ abc")]
    Task<Test> TestHeaderAsync(string methodName);


    // AliasAs specifies the alias
    [Headers("a:{ {methodName}}")]
    [PostMapping("/ abc")]
    Task<Test> TestAliasAsAsync([AliasAs("methodName")] string name);
}

await TestFeign.TestAsync ("abc");
>>> post to http: //localhost:5001/home/abc

await TestFeign.TestAliasAsAsync ("abc");
>>> post, http: //localhost:5001/home/abc

await TestFeign.TestHeaderAsync ("abc");
>>> post, http: //localhost:5001/home/abc, and the request header is"a:abc"
```

### 5.2 Paramètres spéciaux dans la méthode

#### 5.2.1 Les paramètres ajoutent des annotations de requête

Une fois l'annotation de requête ajoutée au paramètre, la valeur du paramètre sera ajoutée à l'URL sous la forme key1=value1&key2=value2 .

```csharp
[FeignClient(Url = "http://localhost:5001/home", IsIgnoreHttpsCertificateValidate = true, InterceptorType = typeof(MyRequestInterceptor), Timeout = 100)]
public interface ITestFeign
{
    [GetMapping("/ TestQuery")]
    Task<Test> TestQuery([Query] string name);

    [GetMapping("/ TestQueryWithClass")]
    Task<Test> TestQueryWithClass([Query] Test tt);
}

await TestFeign.TestQuery ("abc");
>>> get, http: //localhost:5001/home/TestQuery?name=abc

await TestFeign.TestQueryWithClass (new Test() { Name ="abc", Age = 3 });
>>> get, http: //localhost:5001/home/TestQueryWithClass?Name=abc&Age=3
```

##### 5.2.1.1 L'annotation Query est utilisée avec l'annotation Embedded, et la classe d'annotation Embedded peut être ajoutée en tant que paramètre dans son ensemble

```csharp
public class EmbeddedTest2
{
    public int Age { get; set; }
}

public class EmbeddedTest3
{
    public string Name { get; set; }
    [Embedded]
    public EmbeddedTest2 Test { get; set; }
}

[FeignClient(Url = "http://localhost:5001/home")]
public interface ITestFeign
{
    ///<summary>
    /// Test the Embedded annotation, indicating whether the parameter is embedded, the test is embedded
    ///</summary>
    ///<param name ="tt"></param>
    ///<returns></returns>
    [GetMapping("/ testEmbedded")]
    Task<string> TestEmbedded([Query] EmbeddedTest3 tt);
}
    
await testFeign.TestEmbedded(new EmbeddedTest3()
{
    Name = "sb",
    Test = new EmbeddedTest2()
    {
        Age = 3
    }
});

>>> get, http: //localhost:5001/home/testEmbedded?Name=sb&Test=%7B%22Age%22%3A%223%22%7D

```

S'il n'y a pas d'annotation intégrée, la demande devient

```csharp
>>> get, http: //localhost:5001/home/testEmbedded?Name=sb&Age=3
```

#### 5.2.2 Les paramètres ajoutent des annotations Body (BodySerializationKind.Form)

Cela équivaut à simuler la soumission du formulaire en html. La valeur du paramètre sera encodée en URL et ajoutée à la charge utile (corps) sous la forme clé1=valeur1&clé2=valeur2.

```csharp
[FeignClient(Url = "http://localhost:5001/home", IsIgnoreHttpsCertificateValidate = true, InterceptorType = typeof(MyRequestInterceptor), Timeout = 100)]
public interface ITestFeign
{
    [PostMapping("/form")]
    Task<Test> TestForm([Body(BodySerializationKind.Form)] Test tt);
}

await TestFeign.TestForm(new Test() { Name = "abc", Age = 3 });
>>> post, http: //localhost:5001/home/form, and the value in the body is Name= abc&Age =3
```

#### 5.2.3 Les paramètres ajoutent des annotations Body (BodySerializationKind.Json)

Autrement dit, soumettez-le sous la forme application/ json , et la valeur du paramètre sera sérialisée par json et ajoutée à la charge utile (corps). De même, si le champ de la classe a un alias, vous pouvez également utiliser l'annotation AliasAs .

```csharp
[FeignClient(Url = "http://localhost:5001/home", IsIgnoreHttpsCertificateValidate = true, InterceptorType = typeof(MyRequestInterceptor), Timeout = 100)]
public interface ITestFeign
{
    [PostMapping("/ json")]
    Task<Test> TestJson([Body(BodySerializationKind.Json)] Test tt);
}

await TestFeign.TestJson(new Test() { Name = "abc", Age = 3 });
>>> post, http: //localhost:5001/home/json, and the value in the body is {"Name":"abc","Age":3}
```

#### 5.2.4 Utiliser la classe spéciale HeaderCollection comme paramètre de méthode pour ajouter des en-têtes de requête par lots

```csharp
[FeignClient(Url = "http://localhost:5001/home", IsIgnoreHttpsCertificateValidate = true, InterceptorType = typeof(MyRequestInterceptor), Timeout = 100)]
public interface ITestFeign
{
    [PostMapping("/ json")]
    Task<Test> TestJson([Body(BodySerializationKind.Json)] Test tt, HeaderCollection headers);
}

var headerCollection = new HeaderCollection()
{ new KeyValuePair<string , string>("a","a"),
    new KeyValuePair<string , string>("b","b") };

await TestFeign.TestJson(new Test() { Name = "abc", Age = 3 }, headerCollection);
>>> post, http: //localhost:5001/home/json, and the value in the body is {"Name":"abc","Age":3}, and the header is"a:a"and"b: b"
```

#### 5.2.5 Utilisez la classe spéciale BasicAuthorization comme paramètre de méthode pour ajouter l'en-tête de demande d'autorisation pour l'authentification de base

```csharp
[FeignClient(Url = "http://localhost:5001/home", IsIgnoreHttpsCertificateValidate = true, InterceptorType = typeof(MyRequestInterceptor), Timeout = 100)]
public interface ITestFeign
{
    [GetMapping("/ testBasicAuthorization")]
    Task<Test> TestBasicAuthorization(BasicAuthorization basicAuthorization);
}

var username = "abc";
var password = "123";

await TestFeign.TestBasicAuthorization(new BasicAuthorization(username, password));
>>> get, http: //localhost:5001/home/testBasicAuthorization, header is"Authorization: Basic YWJjOjEyMw =="
```

#### 5.2.6 Utilisez la classe spéciale MultipartItem comme paramètre de méthode et marquez l'annotation Multipart sur la méthode pour télécharger la pièce jointe

```csharp
[FeignClient(Url = "http://localhost:5001/home", IsIgnoreHttpsCertificateValidate = true, InterceptorType = typeof(MyRequestInterceptor), Timeout = 100)]
public interface ITestFeign
{
    //Only upload the file
    [Multipart]
    [PostMapping("/multipart")]
    Task<Test> MultipartTest(MultipartItem item);
    //While uploading the attachment, you can also attach parameters
    [Multipart]
    [PostMapping("/multipart")]
    Task<Test> MultipartTest([Body(BodySerializationKind.Form)] Test tt, MultipartItem item);
}
// Only upload the file
var basePath = Path.Combine(AppContext.BaseDirectory, "123.txt");
var name = "file";
var fileName = "123.txt";
//Method 1, use byteArray
var byteArray = File.ReadAllBytes(basePath);
var result = await testFeign.MultipartTest(new MultipartItem(byteArray, name, fileName));

//Method 2, use stream
var fileStream = new FileInfo(basePath).OpenRead();
var result = await testFeign.MultipartTest(new MultipartItem(fileStream, name, fileName));

//Method 3, use fileInfo
var result = await testFeign.MultipartTest(new MultipartItem(new FileInfo(basePath), name, fileName));

>>> post, http: //localhost:5001/home/multipart, with attachments in the body
 
 //While uploading the attachment, you can also attach parameters
var basePath = Path.Combine(AppContext.BaseDirectory, "123.txt");
var name = "file";
var fileName = "123.txt";
//Method 1, use byteArray
var byteArray = File.ReadAllBytes(basePath);
var result = await testFeign.MultipartTest(new Test() { Name = "sb", Age = 3 }, new MultipartItem(byteArray, name, fileName));

//Method 2, use stream
var fileStream = new FileInfo(basePath).OpenRead();
var result = await testFeign.MultipartTest(new Test() { Name = "sb", Age = 3 }, new MultipartItem(fileStream, name, fileName));

//Method 3, use fileInfo
var result = await testFeign.MultipartTest(new Test() { Name = "sb", Age = 3 }, new MultipartItem(new FileInfo(basePath), name, fileName));
 
>>> post, http: //localhost:5001/home/multipart, and the value in the body is Name= abc&Age =3, and it has attachments
```

#### 5.2.7 Utilisez la classe Stream comme type de retour de la méthode pour recevoir des données en continu, telles que le téléchargement de fichiers.

```csharp
[FeignClient(Url = "http://localhost:5001/home", IsIgnoreHttpsCertificateValidate = true, InterceptorType = typeof(MyRequestInterceptor), Timeout = 100)]
public interface ITestFeign
{
    [GetMapping("/ downLoadWithStream")]
    Task<Stream> TestDownLoadWithStream();
}

using var streamResult = await testFeign.TestDownLoadStream();
using var newfile = new FileInfo("D:\\123.txt").OpenWrite();
streamResult.CopyTo(newfile);

>>> get, http: //localhost:5001/home/downLoadWithStream, the return value is streaming data, and then it can be saved as a file.
```

#### 5.2.8 Utilisez la classe HttpResponseMessage comme type de retour de la méthode pour obtenir le message de réponse le plus original.

```csharp
[FeignClient(Url = "http://localhost:5001/home", IsIgnoreHttpsCertificateValidate = true, InterceptorType = typeof(MyRequestInterceptor), Timeout = 100)]
public interface ITestFeign
{
    [GetMapping("/test")]
    Task<HttpResponseMessage> Test();
}

var rawResult = await testFeign.Test();

>>> get, http: //localhost:5001/home/Test, the return value is the original return data of httpclient .
```

#### 5.2.9 Utilisez la classe Task comme type de retour de la méthode, c'est-à-dire qu'aucune valeur de retour n'est requise.

```csharp
[FeignClient(Url = "http://localhost:5001/home", IsIgnoreHttpsCertificateValidate = true, InterceptorType = typeof(MyRequestInterceptor), Timeout = 100)]
public interface ITestFeign
{
    [GetMapping("/test")]
    Task Test();
}

await testFeign.Test();

>>> get, http: //localhost:5001/home/Test, ignore the return value
```

## 6.Microservice - accès aux nacos

### 6.1 Ajouter la configuration nacos dans le fichier de configuration

dans le fichier de configuration appsettings.json / appsettings.Development.json

```json
"nacos": {
    //--------Using nacos , serviceAddress and namespaceId are required------
    // nacos service address, such as http://172.16.189.242:8848
    "serviceAddress":"http://172.16.189.242:8848/",
    // Namespace id, such as 832e754e-e845-47db-8acc-46ae3819b638 or public
   "namespaceId":"dfd8de72-e5ec-4595-91d4-49382f500edf",

    //--------If you just access the microservices in nacos , you only need to configure lbStrategy , defaultNacosGroupName and defaultNacosNamespaceId are optional ------
       //Client load balancing algorithm, there are multiple instances under one service, lbStrategy is used to select instances under the service, the default is Random (random), you can also choose WeightRandom (random after being weighted according to the service weight)
      "lbStrategy":"Random",
       // defaultNacosGroupName , optional, is the default value of NacosGroupName in the FeignClient annotation , if it is empty, it defaults to DEFAULT_GROUP
      "defaultNacosGroupName":"",
       // defaultNacosNamespaceId , optional, is the default value of NacosNamespaceId in the FeignClient annotation , if it is empty, it defaults to public
      "defaultNacosNamespaceId":"",

    //--------If you need to use the nacos configuration center, ConfigurationOption is required------
   "configurationOption": {
        //Configuration grouping
        "groupName":"DEFAULT_GROUP",
      //configured dataId ,
     "dataId":"prd"
},

    //-------If you want to register this application as a service instance, all parameters need to be configured --------------

    //Do you want to register the application as a service instance
   "registerInstance": true ,

    //The name of the service to be registered
   "serviceName":"test",
    //Service group name
   "groupName":"DEFAULT_GROUP",
    //Weight, there are multiple instances under one service, the higher the weight, the greater the probability of accessing the instance, for example, if some instances are located on a server with high configuration, then the weight can be higher, and more traffic will be diverted to the instance, which is the same as the above The parameter lbStrategy is set to WeightRandom for use with
   "weight": 1 ,
    //The external network protocol of this application, http or https
   "protocol":"http",
    //The external port number of this application, such as 5000
   "port": 5000

}
```

### 6.2 Accéder au centre de configuration nacos

L'accès au centre de configuration nacos est très simple, il suffit d'ajouter une ligne .UseNacosConfiguration () dans Program.cs , prend actuellement en charge le format json, le format xml et le format yaml.

net core 3.1 exemple est le suivant

```csharp
public static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .UseNacosConfiguration()
        .ConfigureWebHostDefaults(webBuilder =>
        {
            webBuilder.UseStartup<Startup>().UseUrls("http://*:5001");
        });
```

L'exemple net6 est le suivant

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Host.UseNacosConfiguration();
```

### 6.3 Accéder au centre de service nacos

#### 6.3.1 Ajouter une configuration dans StartUp.cs

Si l'application actuelle est enregistrée en tant qu'instance de microservice, cette étape est terminée et feign enregistrera automatiquement l'application en tant qu'instance de microservice en fonction de la configuration dans le fichier de configuration. Si cette application souhaite appeler l'interface de microservice, veuillez consulter 6.3 .2

```csharp
services.AddSummerBoot();
services.AddSummerBootFeign(it =>
{
    it.AddNacos(Configuration);
});
```

#### 6.3.2 Définir l'interface d'appel des microservices

Définissez le nom du microservice ServiceName , le nom de groupe NacosGroupName (vous pouvez remplir le nom de groupe global par défaut dans le fichier de configuration nacos:defaultNacosGroupName , si vous ne le remplissez pas, il est par défaut à DEFAULT_GROUP), l'espace de noms NacosNamespaceId (vous peut remplir l'espace de noms global par défaut dans le fichier de configuration nacos:defaultNacosNamespaceId , s'il n'est pas rempli, il est par défaut public), et MicroServiceMode est défini sur true. L'url n'a pas besoin d'être configuré, et le reste est identique à la normale interface simulée.

```csharp
[FeignClient(ServiceName = "test", MicroServiceMode = true, NacosGroupName = "DEFAULT_GROUP", NacosNamespaceId = "dfd8de72-e5ec-4595-91d4-49382f500edf")]
public interface IFeignService
{
    [GetMapping("/home/index")]
    Task<string> TestGet();
}
```

Dans le même temps, ServiceName , NacosGroupName et NacosNamespaceId prennent également en charge la lecture à partir de fichiers de configuration, tels que

```json
{
    "ServiceName":"test",
    "NacosGroupName":"DEFAULT_GROUP",
    "NacosNamespaceId":"dfd8de72-e5ec-4595-91d4-49382f500edf"
}
```

```csharp
[FeignClient(ServiceName = "${ ServiceName }", MicroServiceMode = true, NacosGroupName = "${ NacosGroupName }", NacosNamespaceId = "${ NacosNamespaceId }")]
public interface IFeignService
{
    [GetMapping("/home/index")]
    Task<string> TestGet();
}
```

## 7.Utilisation des cookies en contexte

Dans le mode unité de travail en feigne, les cookies peuvent être définis dans le contexte, de sorte que l'interface apportera automatiquement des cookies lorsque l'interface initie une requête http dans le contexte. Pour utiliser le mode unité de travail, vous devez injecter le IFeignUnitOfWork interface, puis procédez comme suit :

```csharp
var feignUnitOfWork = serviceProvider.GetRequiredService<IFeignUnitOfWork>();
//Open the context
feignUnitOfWork.BeginCookie();
//add cookie
feignUnitOfWork.AddCookie("http://localhost:5001/home/TestCookieContainer2", "abc =1");
await testFeign.TestCookieContainer2();
//end context
feignUnitOfWork.StopCookie();

```

Dans le même temps, si l'interface renvoie les informations de configuration du cookie, l'unité de travail enregistre également le cookie, et lorsque l'interface dans la portée du contexte initie l'accès http, elle apporte automatiquement les informations du cookie.Un scénario typique est que nous d'abord Une fois la première interface connectée, l'interface nous renverra le cookie.Lorsque nous visitons l'interface suivante, nous devons apporter le cookie qui nous est renvoyé par la première interface. :

```csharp
var feignUnitOfWork = serviceProvider.GetRequiredService<IFeignUnitOfWork>();
//Open the context
feignUnitOfWork.BeginCookie();

// get cookie after login
await testFeign.LoginAsync("sb", "123");
    / / Automatically bring the cookie after login when requesting
await testFeign.TestCookieContainer3();
//end context
feignUnitOfWork.StopCookie();
```

# SummerBoot utilise le cache pour les opérations de cache

## 1.Service d'enregistrement

Le cache est divisé en cache mémoire et cache redis. La méthode d'enregistrement du cache mémoire est la suivante :

```csharp
services.AddSummerBoot();
services.AddSummerBootCache(it => it.UseMemory());
```

La méthode d'enregistrement du cache Redis est la suivante, connectionString est la chaîne de connexion Redis :

```csharp
services.AddSummerBoot();
services.AddSummerBootCache(it =>
{
    it.UseRedis(connectionString);
});
```

## 2. Interface ICache

L'interface ICache a principalement les méthodes suivantes et les méthodes asynchrones correspondantes

```csharp
///<summary>
/// Absolute time cache, the cache value becomes invalid after a fixed time
///</summary>
///<typeparam name ="T"></typeparam>
///<param name ="key"></param>
///<param name ="value"></param>
///<param name ="absoluteExpiration"></param>
///<returns></returns>
bool SetValueWithAbsolute<T>(string key, T value, TimeSpan absoluteExpiration);
///<summary>
/// Sliding time cache, if there is a hit within the time, the time will continue to be extended, and the cache value will be invalid if it is not hit
///</summary>
///<typeparam name ="T"></typeparam>
///<param name ="key"></param>
///<param name ="value"></param>
///<param name ="slidingExpiration"></param>
///<returns></returns>
bool SetValueWithSliding<T>(string key, T value, TimeSpan slidingExpiration);
///<summary>
/// get value
///</summary>
///<typeparam name ="T"></typeparam>
///<param name ="key"></param>
///<returns></returns>
CacheEntity<T> GetValue<T>(string key);
///<summary>
/// remove value
///</summary>
///<param name ="key"></param>
///<returns></returns>
bool Remove(string key);
```

## 3.Après avoir injecté l'interface, elle peut être utilisée

```csharp
var cache = serviceProvider.GetRequiredService<ICache>();
    / / Set a fixed time cache
cache.SetValueWithAbsolute("test", "test", TimeSpan.FromSeconds(3));
//Set sliding time cache
var cache = serviceProvider.GetRequiredService<ICache>();
cache.SetValueWithSliding("test", "test", TimeSpan.FromSeconds(3));
// get cache
var value = cache.GetValue<string>("test");
// remove cache
cache.Remove("test");
```

Conception conviviale dans #SummerBoot
fonction fournie avec net core mvc. Et si nous voulions configurer l'adresse IP et le port de l'application Web dans appsettings.json ? Écrivez directement dans appsettings.json

    {
    "urls":"http://localhost:7002;http://localhost:7012"
    }

2\. L'annotation AutoRegister est utilisée pour permettre au framework d'enregistrer automatiquement l'interface et la classe d'implémentation de l'interface dans le conteneur IOC, et de la marquer sur la classe d'implémentation. Les paramètres de l'annotation sont le type de l'interface personnalisée correspondant à cette classe et le cycle de vie du service ServiceLifetime (le cycle par défaut est au niveau de la portée), l'utilisation est la suivante :

```csharp
public interface ITest
{

}

[AutoRegister(typeof(ITest), ServiceLifetime.Transient)]
public class Test : ITest
{

}
```

3\. Classe d'emballage de valeur de retour d'interface ApiResult, y compris code, msg et données, 3 champs, de sorte que la valeur de retour de l'ensemble du système soit unifiée et ordonnée, ce qui est propice à l'interception unifiée et au fonctionnement unifié du front-end. pour l'utiliser est la suivante :

```csharp
[HttpPost("CreateServerConfigAsync")]
public async Task<ApiResult<bool>> CreateServerConfigAsync(ServerConfigDto dto)
{
    var result = await serverConfigService.CreateServerConfigAsync(dto);
    return ApiResult<bool>.Ok(result);
}
```

4\. Certaines opérations améliorées pour net core mvc, y compris l'intercepteur d'erreur global et le traitement après l'échec de la vérification des paramètres d'interface, coopèrent avec ApiResult, de sorte que lorsque le système signale une erreur, il peut également revenir de manière uniforme. L'utilisation est la suivante .Tout d'abord, enregistrez-le dans le service de démarrage, notez qu'il doit être placé après l'enregistrement mvc :

```csharp
services.AddControllersWithViews();
services.AddSummerBootMvcExtension(it =>
{
    // Whether to enable global error handling
    it.UseGlobalExceptionHandle = true;
    //Whether to enable parameter verification processing
    it.UseValidateParameterHandle = true;
});
```

4.1 L'effet de l'utilisation de l'intercepteur d'erreur global
Nous pouvons lancer une erreur directement dans le code métier, et l'intercepteur d'erreurs global captera l'erreur, puis la renverra au frontal dans un format unifié. Le code métier est le suivant :

```csharp
private void ValidateData(EnvConfigDto dto)
{
    if (dto == null)
    {
        throw new ArgumentNullException("Argument cannot be empty");
    }
    if (dto.ServerConfigs == null || dto.ServerConfigs.Count == 0)
    {
        throw new ArgumentNullException("There is no server configured in the environment");
    }
}
```

Si une erreur est signalée dans le code métier, la valeur de retour est la suivante :

```csharp
{
 "code": 40000 ,
 "msg":"Value cannot be null.(Parameter 'There is no server configured in the environment')",
 "data": null
}
```

4.2 L'effet du traitement après l'échec de la vérification des paramètres d'interface
ajouter des annotations de vérification dans le paramètre dto de l'interface, le code est le suivant

```csharp
public class EnvConfigDto : BaseEntity
{
    ///<summary>
    /// Environment name
    ///</summary>
    [Required(AllowEmptyStrings = false, ErrorMessage = "Environment name cannot be empty")]
    public string Name { get; set; }
    ///<summary>
    /// The corresponding server in the environment
    ///</summary>
    [NotMapped]
    public List<int> ServerConfigs _ { get ; set ; }
}
```

Si la vérification des paramètres échoue, la valeur de retour est la suivante :

```csharp
{
 "code": 40000 ,
 "msg":"Environment name cannot be empty",
 "data": null
}
```

5.QueryCondition, la combinaison des conditions de requête lambda, résout le problème du filtrage et de l'interrogation à partir du front-end. En plus des méthodes de base And et Or, une méthode plus humanisée est ajoutée. du front-end ont des types de chaîne. S'ils ont des valeurs, ils sont ajoutés à la condition de requête, donc deux méthodes sont spécialement extraites, y compris AndIfStringIsNotEmpty (si la chaîne n'est pas vide, l'opération et est effectuée, sinon l'expression d'origine est renvoyé), OrIfStringIsNotEmpty (si la chaîne n'est pas vide, alors Perform ou opération, sinon retour à l'expression d'origine),
Dans le même temps, les attributs de dto peuvent également être de type nullable, c'est-à-dire de type nullable, tel que int? test représente si l'utilisateur remplit une certaine condition de filtre, si hasValue est ajouté à la condition de requête, donc deux méthodes sont spécialement extraites, AndIfNullableHasValue (si la valeur nullable n'est pas vide, l'opération AND est effectuée, sinon l'expression d'origine est renvoyée ), OrIfNullableHasValue (si la valeur nullable n'est pas vide, l'opération AND est effectuée, sinon l'expression d'origine est renvoyée) l'utilisation est la suivante :

```csharp
// dto
public class ServerConfigPageDto : IPageable
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    ///<summary>
    /// ip address
    ///</summary>
    public string Ip { get; set; }
    ///<summary>
    /// connection name
    ///</summary>
    public string ConnectionName { get; set; }

    public int? Test { get; set; }
}
//condition
var queryCondition = QueryCondition.True<ServerConfig>()
    .And(it => it.Active == 1)
    //If the string is not empty, perform the and operation, otherwise return the original expression
    .AndIfStringIsNotEmpty(dto.Ip, it => it.Ip.Contains(dto.Ip))
    //If the nullable value is not empty, perform the and operation, otherwise return to the original expression
    .AndIfNullableHasValue(dto.Test, it => it.Test == dto.Test)
    .AndIfStringIsNotEmpty(dto.ConnectionName, it => it.ConnectionName.Contains(dto.ConnectionName));

var queryResult = await serverConfigRepository.Where(queryCondition)
    .Skip((dto.PageNumber - 1) * dto.PageSize).Take(dto.PageSize).ToPageAsync();
```
