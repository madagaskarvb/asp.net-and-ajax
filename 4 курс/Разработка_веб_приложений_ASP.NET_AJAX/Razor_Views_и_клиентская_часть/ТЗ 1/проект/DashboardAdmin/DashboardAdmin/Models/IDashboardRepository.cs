using DashboardAdmin.Models;
using System.Collections.Generic;

namespace DashboardAdmin.Models
{
    public interface IDashboardRepository
    {
        IEnumerable<DashboardCard> GetAllCards();
        DashboardCard? GetById(int id);
        IEnumerable<DashboardCard> GetLatest(int count);
    }
}