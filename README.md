# Observable.Repository
Event driven approach to store, aggregate and manage data
## Introduction
Repository library allows you to store and build complex views by aggregating atomic data.
Repository is an event driven approach to support cascading aggregations.
As a relational database you can define aggregation/view with different join cardinality:
- 1..[0|1] as a left outer join
- 1..*

You can aggregate many data in one single tuple and the framework allows you to choose the kind of join for each aggregation. Since it's an event driven approach the framework needs producers to inject data and supports cascading between repositories, that means a repository source can be another repository, instead of a producer. See below an example of repository structure:

![Introduction.png](https://raw.githubusercontent.com/fdieulle/Observable.Repository/master/docs/Images/Introduction.png)

## Repository
The main repository interface is `IRepository<TKey, TValue>`
- It's **Linq compatible** because it inherits from `IEnumerable<KeyValue<TKey, TValue>>`.
- It's an **event** because it inherits from `IObservable<RepositoryNotification<KeyValue<TKey, TValue>>>`.
- It's able to **manage view as list** because it exposes method `Subscribe(IList<TValue> list, ...)`
- It's **queryable as a dictionary** because it exposes methods as `TryGetValue(TKey key, out TValue value)` or `this[TKey key]`.

A repository content can't be modified directly from the interface. The only way to modify its content is by publishing data from its producer's sources.
You can build another repository from this interface, that means that the current repository will be the main source of the new one.

## Producer
A producer publish a specific data type, you can also optionnaly named it. The main interface of a producer is `IObservable<RepositoryNoification<T>>`. A `RepositoryNotification<T>` data provides the following properties:
- **Action**: Defines the published event purpose, can take following values `Add`,`Update`,`Remove` or `Reload`.
- **OldItems**: Defines old items concerned about published event.
- **NewItems**: Defines new items concerned about published event.

As you can see in the previous schema a repository source can be a producer event but also another repository, because of `IObservable<T>` implementation.

## Container
A main interface allows you to build and register all repositories and producers: `IRepositoryContainer`. The container store all registered repositories and producers. Both can be named if the type isn't enough to distinguish them. It's also the entry point to build repository from scratch and access to fluent builders.

## Fluent repository builder
It can be hard to configure a repository and all behaviors you want to define. To help you doing that, a smart and fluent builder is available. This builder is compilator complient so you can't do a type mistake in your configuration, otherwise you can't compile your code. Let's see an example of configuration:

<!-- language: lang-cs -->

	container.Register<int, Client>(client => client.PKClientId);
	container.Build<int, ExecutedOrder, Order>(order => order.PKOrderId)
    	.JoinMany<Trade>()
        	.DefineList(execOrder => execOrder.Trades)
            .RightPrimaryKey(trade => trade.PKTradeId)
            .RightLinkKey(trade => trade.FKOrderId)
            .LeftLinkKey(order => order.PKOrderId)
        .DefineCtor(order => new ExecutedOrder { Order = order })
        .Register();
    container.Build<int, ClientOrder, ExecutedOrder>(execOrder => execOrder.Order.PKOrderId)
        .Join<Client>()
            .RightLinkKey(client => client.PKClientId)
            .LeftLinkKey(execOrder => execOrder.Order.FKClientId)
        .JoinUpdate<Company>()
            .DefineUpdate(clientOrder => company => clientOrder.Company = company)
            .RightLinkKey(company => company.PKCompanyId)
            .LeftLinkKey(execOrder => execOrder.Order.FKCompanyId)
        .Register();

## View
If you want a view on a repository with different criteria like a filter or a data selector, you can do it easily.
For example in a client application context you want to display a list of running orders and you don't want to manage each element operations like Add, Remove, Clear, ... You can subscribe on the repository by given the list/view instance, the repository will manage its state for you.

<!-- language: lang-cs -->

    var list = new List<ClientOrder>();
    var suscription = container.GetRepository<int, ClientOrder>()
        					   .Subscribe(list);
    // The list will be managed until the suscription result will be disposed
    suscription.Dispose();

Be carefull, if you decide to give a list management to a repository, you can't modify it yourself because the repository will fail on the next events. But once the subscription is disposed the list is cleared and free to be reused by user code.

## Technical features and performances:
The framework is fully thread safe.
There is no order to respect between repository/producer definition. All data published is stored if there is a repository which consume it, and if the data isn't tagged as Remove.
The memory foot print is optimized and avoid to generate too many Gen2 garbage collection.
The asymptotic complexity for all atomic operation is O(1) with cascading included. If a producer raise a list of data this operation will be O(n) with n equals to data count. In the worst case we can have an O(n) complexity if a repository raise a remove event for the first element of a managed list view which implements `IList<T>`, with n equals to the managed list view count.
