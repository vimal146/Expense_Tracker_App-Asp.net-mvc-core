using FinanceTracker.Data;
using Microsoft.AspNetCore.Mvc;
using FinanceTracker.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace FinanceTracker.Controllers
{
	[Authorize]
	public class DashboardController : Controller
	{

		private readonly FinanceDbContext _context;

		public DashboardController( FinanceDbContext context)
		{
			
			_context = context;
		}

		
		public async Task<ActionResult> Index()
		{

			DateTime StartDate = DateTime.Today.AddDays(-6);
			DateTime EndDate = DateTime.Today;

			List<Transaction> SelectTransactions = await _context.Transactions
				.Include(x => x.category)
				.Where(y => y.Date >= StartDate && y.Date <= EndDate)
				.ToListAsync();

			//Total Income
			int TotalIncome = SelectTransactions
				.Where(i => i.category.Type == "Income")
				.Sum(j => j.Amount);
			ViewBag.TotalIncome = TotalIncome.ToString("C0");


			//Total Expense
			int TotalExpense = SelectTransactions
				.Where(i => i.category.Type == "Expense")
				.Sum(j => j.Amount);
			ViewBag.TotalExpense = TotalExpense.ToString("C0");

			//Balance
			int Balance = TotalIncome - TotalExpense;
			ViewBag.Balance = Balance.ToString("C0");

			//doughnut chart-expense by category
			ViewBag.DoughnutChartData = SelectTransactions
				.Where(i => i.category.Type == "Expense")
				.GroupBy(j => j.category.CategoryId)
				.Select(k => new
				{
					categoryTitleWithIcon = k.First().category.Icon + " " + k.First().category.Title,
					amount = k.Sum(j => j.Amount),
					formattedAmount = k.Sum(j => j.Amount).ToString("C0"),
				})
				.OrderBy(l => l.amount)
				.ToList();

			//spline chart -Income vs Expense

			List<SplineChartData> IncomeSummary = SelectTransactions
				.Where(i => i.category.Type == "Income")
				.GroupBy(j => j.Date)
				.Select(k => new SplineChartData()
				{
					day = k.First().Date.ToString("dd-MMM"),
					income = k.Sum(l => l.Amount)
				})
				.ToList();
			//Expense
			List<SplineChartData> ExpenseSummary = SelectTransactions
				.Where(i => i.category.Type == "Expense")
				.GroupBy(j => j.Date)
				.Select(k => new SplineChartData()
				{
					day = k.First().Date.ToString("dd-MMM"),
					expense = k.Sum(l => l.Amount)
				})
				.ToList();

			//combine Income & Expense
			string[] Last7Days = Enumerable.Range(0, 7)
				.Select(i => StartDate.AddDays(i).ToString("dd-MMM"))
				.ToArray();

			ViewBag.SplineChartData = from day in Last7Days
									  join income in IncomeSummary on day equals income.day into dayIncomeJoined
									  from income in dayIncomeJoined.DefaultIfEmpty()
									  join expense in ExpenseSummary on day equals expense.day into expenseJoined
									  from expense in expenseJoined.DefaultIfEmpty()
									  select new
									  {
										  day = day,
										  income = income == null ? 0 : income.income,
										  expense = expense == null ? 0 : expense.expense,
									  };

			ViewBag.RecentTransactions = await _context.Transactions
				.Include(i => i.category)
				.OrderByDescending(j => j.Date)
				.Take(5)
				.ToListAsync();

			return View();
		}

		public class SplineChartData
		{
			public string day;
			public int income;
			public int expense;
		}
	}
}
