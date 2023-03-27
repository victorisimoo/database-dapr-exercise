using Autofac;
using Npgsql;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.Data;
using chessAPI.dataAccess.providers.postgreSQL;
using chessAPI.dataAccess.interfaces;
using chessAPI.dataAccess.common;
using chessAPI.dataAccess.queries.postgreSQL;
using chessAPI.dataAccess.queries;
using chessAPI.dataAccess.repositores;
using chessAPI.business.interfaces;
using chessAPI.business.impl;
using MongoDB.Bson;
using chessAPI.dataAccess.models;

namespace chessAPI;

public sealed class dependencyInjection<TC, TI> : Module
where TI : struct, IEquatable<TI>
        where TC : struct
{
    protected override void Load(ContainerBuilder builder)
    {
        base.Load(builder);

        //Relational datastore
        builder.Register(c => new NpgsqlConnection(c.Resolve<IOptions<connectionStrings>>().Value.relationalDBConn))
            .InstancePerLifetimeScope()
            .As<IDbConnection>();

        //Non-relational datastore
        builder.Register(c=> new MongoClient(c.Resolve<IOptions<connectionStrings>>().Value.mongoDbConn))
            .SingleInstance()
            .As<IMongoClient>();
        
        builder.Register(c=> c.Resolve<IMongoClient>().GetDatabase("chess"))
            .SingleInstance()
            .As<IMongoDatabase>();

        #region "Low level relational DAL Infrastructure"
        builder.Register(c => new clsConcurrency<TC>())
            .SingleInstance()
            .As<IDBConcurrencyHandler<TC>>();
        builder.Register(c => new clsRelationalContext<TC>(c.Resolve<IDbConnection>(),
                                                           c.Resolve<ILogger<clsRelationalContext<TC>>>(),
                                                           c.Resolve<IDBConcurrencyHandler<TC>>()))
                .InstancePerLifetimeScope()
                .As<IRelationalContext<TC>>();
        #endregion

        #region "MongoDb Collections"
        builder.Register(c=> c.Resolve<IMongoClient>().GetDatabase("chess").GetCollection<clsGameEntityModel>("games"))
            .InstancePerDependency()
            .As<IMongoCollection<clsGameEntityModel>>();
        #endregion

        #region "Queries"
        builder.Register(c => new qPlayer())
          .SingleInstance()
          .As<IQPlayer>();
        #endregion

        #region "Relational Repositories"
        builder.Register(c => new clsPlayerRepository<TI, TC>(c.Resolve<IRelationalContext<TC>>(),
                                                              c.Resolve<IQPlayer>(),
                                                              c.Resolve<ILogger<clsPlayerRepository<TI, TC>>>()))
               .InstancePerDependency()
               .As<IPlayerRepository<TI, TC>>();
        #endregion

        #region "Non-Relational Repositories"
        builder.Register(c=> new clsGameRepository(c.Resolve<IMongoCollection<clsGameEntityModel>>()))
            .InstancePerDependency()
            .As<IGameRepository>();
        #endregion

        #region "Kaizen Entity Factories"
        builder.Register<Func<IPlayerRepository<TI, TC>>>(delegate (IComponentContext context)
        {
            IComponentContext cc = context.Resolve<IComponentContext>();
            return cc.Resolve<IPlayerRepository<TI, TC>>;
        });
        #endregion

        #region "Business classes"
        builder.Register(c => new clsPlayerBusiness<TI, TC>(c.Resolve<IPlayerRepository<TI, TC>>()))
               .InstancePerDependency()
               .As<IPlayerBusiness<TI>>();
        builder.Register(c => new clsGameBusiness(c.Resolve<IGameRepository>()))
               .InstancePerDependency()
               .As<IGameBusiness>();
        #endregion
    }
}