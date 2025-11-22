using ModelContextProtocol.Server;

namespace McpSample.Server.Mcp;

/// <summary>
/// Sample MCP tools for basic calculations
/// </summary>
[McpServerToolType]
public class CalculatorTools
{
    private readonly ILogger<CalculatorTools> _logger;

    public CalculatorTools(ILogger<CalculatorTools> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Adds two numbers together
    /// </summary>
    [McpServerTool]
    public int Add(int a, int b)
    {
        _logger.LogInformation("MCP Tool: Add - {A} + {B}", a, b);
        return a + b;
    }

    /// <summary>
    /// Subtracts the second number from the first number
    /// </summary>
    [McpServerTool]
    public int Subtract(int a, int b)
    {
        _logger.LogInformation("MCP Tool: Subtract - {A} - {B}", a, b);
        return a - b;
    }

    /// <summary>
    /// Multiplies two numbers together
    /// </summary>
    [McpServerTool]
    public int Multiply(int a, int b)
    {
        _logger.LogInformation("MCP Tool: Multiply - {A} * {B}", a, b);
        return a * b;
    }

    /// <summary>
    /// Divides the first number by the second number
    /// </summary>
    [McpServerTool]
    public double Divide(double a, double b)
    {
        _logger.LogInformation("MCP Tool: Divide - {A} / {B}", a, b);
        
        if (b == 0)
        {
            throw new ArgumentException("Cannot divide by zero", nameof(b));
        }
        
        return a / b;
    }

    /// <summary>
    /// Calculates a number raised to a power
    /// </summary>
    [McpServerTool]
    public double Power(double baseNumber, double exponent)
    {
        _logger.LogInformation("MCP Tool: Power - {Base} ^ {Exponent}", baseNumber, exponent);
        return Math.Pow(baseNumber, exponent);
    }

    /// <summary>
    /// Calculates the square root of a number
    /// </summary>
    [McpServerTool]
    public double SquareRoot(double number)
    {
        _logger.LogInformation("MCP Tool: SquareRoot - √{Number}", number);
        
        if (number < 0)
        {
            throw new ArgumentException("Cannot calculate square root of negative number", nameof(number));
        }
        
        return Math.Sqrt(number);
    }
}
