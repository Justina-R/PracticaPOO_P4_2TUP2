using Microsoft.AspNetCore.Mvc;
using Core.Entities;

namespace Web.Controllers;

[ApiController]
[Route("[controller]")]
public class BankAccountController : ControllerBase
{
    private static List<BankAccount> accounts = new List<BankAccount>();

    [HttpPost("[Action]")]
    public ActionResult<string> CreateBankAccount([FromQuery] string name, [FromQuery] decimal initialBalance, [FromQuery] AccountType accountType, [FromQuery] decimal? creditLimit = null, [FromQuery] decimal? monthlyDeposit = null)
    {
        try
        {
            var account = new BankAccount(name, initialBalance);
            switch (accountType)
            {
                case AccountType.LineOfCredit: // -- Opción 0 --
                    if (creditLimit == null)
                        return BadRequest("Credit limit is required for a Line of Credit account.");
                    account = new LineOfCreditAccount(name, initialBalance, creditLimit.Value);
                    break;
                case AccountType.GiftCard: // -- Opción 1 --
                    account = new GiftCardAccount(name, initialBalance, monthlyDeposit ?? 0);
                    break;
                case AccountType.InterestEarning: // -- Opción 2 --
                    account = new InterestEarningAccount(name, initialBalance);
                    break;
            }

            accounts.Add(account);

            return CreatedAtAction("GetAccountById", new { account.Id }, account);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error interno del servidor: {ex.Message}");
        }
    }


    [HttpGet("{id}")]
    public ActionResult<BankAccount> GetAccountById([FromRoute] int id)
    {
        try
        {
            var account = accounts.FirstOrDefault(a => a.Id == id);

            if (account == null)
            {
                return NotFound($"No se encontró una cuenta con el ID {id}.");
            }

            return account;
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error interno del servidor: {ex.Message}");
        }
    }


    [HttpPost("{accountId}/[Action]")]
    public ActionResult<string> MakeDeposit([FromQuery] decimal amount, [FromQuery] string note, [FromRoute] int accountId)
    {
        try
        {
            var account = accounts.FirstOrDefault(a => a.Id == accountId);

            if (account == null)
                return NotFound("Cuenta no encontrada.");

            account.MakeDeposit(amount, DateTime.Now, note);

            return Ok($"A deposit of ${amount} was made in account {account.Id}.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error interno del servidor: {ex.Message}");
        }
    }

    [HttpPost("{accountId}/[Action]")]
    public ActionResult<string> MakeWithdrawal([FromQuery] decimal amount, [FromQuery] string note, [FromRoute] int accountId)
    {
        try
        {
            var account = accounts.FirstOrDefault(a => a.Id == accountId);

            if (account == null)
                return NotFound("Cuenta no encontrada.");

            account.MakeWithdrawal(amount, DateTime.Now, note);

            return Ok($"A withdrawal of ${amount} was made in account {account.Id}.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error interno del servidor: {ex.Message}");
        }
    }

    [HttpGet("{accountId}/[Action]")]
    public ActionResult<string> GetBalance([FromRoute] int accountId)
    {
        try
        {
            var account = accounts.FirstOrDefault(a => a.Id == accountId);

            if (account == null)
                return NotFound("Cuenta no encontrada.");

            return Ok($"The balance in account {account.Id} is ${account.Balance}.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error interno del servidor: {ex.Message}");
        }
    }
    [HttpGet("{accountId}/[Action]")]
    public IActionResult GetAccountHistory([FromRoute] int accountId)
    {
        try
        {
            var account = accounts.FirstOrDefault(a => a.Id == accountId);

            if (account == null)
                return NotFound("Cuenta no encontrada.");

            var history = account.GetAccountHistory();

            return Ok(history);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error interno del servidor: {ex.Message}");
        }
    }


    //ENDPOINT CIERRE DE MES
    [HttpGet("{id}/[Action]")]
    public ActionResult<BankAccount> PerformMonthEndTransactions([FromRoute] int id)
    {
        try
        {
            var account = accounts.FirstOrDefault(a => a.Id == id);

            if (account == null)
            {
                return NotFound($"No se encontró una cuenta con el ID {id}.");
            }

            account.PerformMonthEndTransactions();

            return account;
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error interno del servidor: {ex.Message}");
        }
    }
}