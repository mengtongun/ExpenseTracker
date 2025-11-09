using ExpenseTracker.Application.Common.Interfaces.Services;
using ExpenseTracker.Application.Contracts.RecurringExpenses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;

namespace ExpenseTrackerApi.Controllers;

[Authorize]
[Route("api/[controller]")]
public class RecurringExpensesController : ApiControllerBase
{
    private readonly IRecurringExpenseService _recurringExpenseService;

    public RecurringExpensesController(IRecurringExpenseService recurringExpenseService)
    {
        _recurringExpenseService = recurringExpenseService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<RecurringExpenseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<RecurringExpenseDto>>> List(CancellationToken cancellationToken)
    {
        var items = await _recurringExpenseService.ListAsync(CurrentUserId, cancellationToken);
        return Ok(items);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(RecurringExpenseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RecurringExpenseDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var recurringExpense = await _recurringExpenseService.GetAsync(CurrentUserId, id, cancellationToken);
        if (recurringExpense is null)
        {
            return NotFound();
        }

        return Ok(recurringExpense);
    }

    [HttpPost]
    [ProducesResponseType(typeof(RecurringExpenseDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<RecurringExpenseDto>> Create([FromBody] CreateRecurringExpenseRequest request, CancellationToken cancellationToken)
    {
        var recurringExpense = await _recurringExpenseService.CreateAsync(CurrentUserId, request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = recurringExpense.Id }, recurringExpense);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(RecurringExpenseDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<RecurringExpenseDto>> Update(Guid id, [FromBody] UpdateRecurringExpenseRequest request, CancellationToken cancellationToken)
    {
        var recurringExpense = await _recurringExpenseService.UpdateAsync(CurrentUserId, id, request, cancellationToken);
        return Ok(recurringExpense);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _recurringExpenseService.DeleteAsync(CurrentUserId, id, cancellationToken);
        return NoContent();
    }

    [HttpPost("process")] // manual trigger for background job fallback
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    public async Task<ActionResult<int>> ProcessDue(CancellationToken cancellationToken)
    {
        var createdCount = await _recurringExpenseService.ProcessDueRecurringExpensesAsync(cancellationToken);
        return Ok(createdCount);
    }
}

