using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Moq;

namespace Tests.Helpers;

public class DbContextMock
{
    public static TContext GetMock<TData, TContext>(List<TData> lstData, Expression<Func<TContext, DbSet<TData>>> dbSetSelectionExpression) where TData : class where TContext : DbContext
    {
        var lstDataQueryable = lstData.AsQueryable();
        var dbSetMock = new Mock<DbSet<TData>>();
        var dbContext = new Mock<TContext>();

        dbSetMock.As<IQueryable<TData>>().Setup(s => s.Provider).Returns(lstDataQueryable.Provider);
        dbSetMock.As<IQueryable<TData>>().Setup(s => s.Expression).Returns(lstDataQueryable.Expression);
        dbSetMock.As<IQueryable<TData>>().Setup(s => s.ElementType).Returns(lstDataQueryable.ElementType);
        dbSetMock.As<IQueryable<TData>>().Setup(s => s.GetEnumerator()).Returns(() => lstDataQueryable.GetEnumerator());
        
        // AddAsync
        dbSetMock.Setup(x => x.AddAsync(It.IsAny<TData>(), default))
            .Callback<TData, CancellationToken>((e, _) => lstData.Add(e))
            .Returns(new ValueTask<EntityEntry<TData>>());
        // FindAsync
        dbSetMock.Setup(x => x.FindAsync(It.IsAny<TData>()))
            .Returns((TData e) => new ValueTask<TData?>(lstData.Find(x => x == e)));

        dbSetMock.Setup(x => x.Remove(It.IsAny<TData>())).Callback<TData>(t => lstData.Remove(t));

        dbContext.Setup(dbSetSelectionExpression).Returns(dbSetMock.Object);

        return dbContext.Object;
    }
}