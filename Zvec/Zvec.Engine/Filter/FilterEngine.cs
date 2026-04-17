// Copyright 2025-present the zvec project — Pure C# Engine
// Filter expression parser and evaluator
//
// Implements a recursive-descent parser that converts SQL-like filter strings
// into an AST (Abstract Syntax Tree), which is then evaluated per-document
// during vector search. Filters are applied post-retrieval: the vector index
// returns candidates by similarity, and the filter engine narrows them down.
//
// Supported expressions:
//   category == "electronics"
//   category == "electronics" AND price > 10.0
//   (category == "books" OR category == "toys") AND price <= 50
//   NOT (category == "food")

using Foundation.AI.Zvec.Engine.Core;

namespace Foundation.AI.Zvec.Engine.Filter;

// ========================================================================
// AST Node Types
// ========================================================================

/// <summary>
/// Base interface for filter expression AST nodes.
/// Each node can evaluate itself against a <see cref="Document"/>
/// to determine if the document passes the filter.
/// The AST forms a tree: logical operators (AND/OR/NOT) at interior nodes,
/// field comparisons at leaf nodes.
/// </summary>
public interface IFilterNode
{
    /// <summary>Evaluate this filter node against a document. Returns true if the document matches.</summary>
    bool Evaluate(Document doc);
}

/// <summary>
/// Comparison: field op value (e.g., price > 10.0)
/// </summary>
public sealed class ComparisonNode : IFilterNode
{
    public string FieldName { get; }
    public ComparisonOp Op { get; }
    public object Value { get; }

    public ComparisonNode(string fieldName, ComparisonOp op, object value)
    {
        FieldName = fieldName;
        Op = op;
        Value = value;
    }

    public bool Evaluate(Document doc)
    {
        doc.Fields.TryGetValue(FieldName, out var fieldVal);

        if (Value == null)
        {
            if (Op == ComparisonOp.Eq) return fieldVal == null;
            if (Op == ComparisonOp.Neq) return fieldVal != null;
            return false;
        }

        if (fieldVal == null) return false;

        return Op switch
        {
            ComparisonOp.Eq => Equals(fieldVal, Value),
            ComparisonOp.Neq => !Equals(fieldVal, Value),
            ComparisonOp.Lt => Compare(fieldVal, Value) < 0,
            ComparisonOp.Lte => Compare(fieldVal, Value) <= 0,
            ComparisonOp.Gt => Compare(fieldVal, Value) > 0,
            ComparisonOp.Gte => Compare(fieldVal, Value) >= 0,
            _ => false
        };
    }

    private static new bool Equals(object a, object b)
    {
        // Handle type mismatches (e.g., int compared with double)
        if (a is IConvertible ca && b is IConvertible cb)
        {
            try
            {
                double da = ca.ToDouble(null);
                double db = cb.ToDouble(null);
                return System.Math.Abs(da - db) < 1e-9;
            }
            catch { }
        }
        return a.ToString() == b.ToString();
    }

    private static int Compare(object a, object b)
    {
        if (a is IConvertible ca && b is IConvertible cb)
        {
            try
            {
                double da = ca.ToDouble(null);
                double db = cb.ToDouble(null);
                return da.CompareTo(db);
            }
            catch { }
        }
        return string.Compare(a.ToString(), b.ToString(), StringComparison.Ordinal);
    }
}

/// <summary>
/// Logical AND: left AND right
/// </summary>
public sealed class AndNode : IFilterNode
{
    public IFilterNode Left { get; }
    public IFilterNode Right { get; }

    public AndNode(IFilterNode left, IFilterNode right) { Left = left; Right = right; }
    public bool Evaluate(Document doc) => Left.Evaluate(doc) && Right.Evaluate(doc);
}

/// <summary>
/// Logical OR: left OR right
/// </summary>
public sealed class OrNode : IFilterNode
{
    public IFilterNode Left { get; }
    public IFilterNode Right { get; }

    public OrNode(IFilterNode left, IFilterNode right) { Left = left; Right = right; }
    public bool Evaluate(Document doc) => Left.Evaluate(doc) || Right.Evaluate(doc);
}

/// <summary>
/// Logical NOT: NOT expr
/// </summary>
public sealed class NotNode : IFilterNode
{
    public IFilterNode Inner { get; }

    public NotNode(IFilterNode inner) { Inner = inner; }
    public bool Evaluate(Document doc) => !Inner.Evaluate(doc);
}

// ========================================================================
// Comparison operators
// ========================================================================

public enum ComparisonOp
{
    Eq,   // ==
    Neq,  // !=
    Lt,   // <
    Lte,  // <=
    Gt,   // >
    Gte   // >=
}

// ========================================================================
// Recursive-descent parser
// ========================================================================

/// <summary>
/// Recursive-descent parser for filter expressions.
///
/// <para><b>Architecture:</b>
/// Tokenizer → Parser → AST. The parser follows standard precedence rules:
/// OR (lowest) → AND → NOT (highest), with parentheses for explicit grouping.</para>
///
/// <para><b>Grammar (EBNF):</b></para>
/// <code>
///   expr     → or_expr
///   or_expr  → and_expr ("OR" and_expr)*
///   and_expr → unary ("AND" unary)*
///   unary    → "NOT" unary | primary
///   primary  → comparison | "(" expr ")"
///   comparison → IDENTIFIER op value
///   op       → "==" | "!=" | "&lt;" | "&lt;=" | "&gt;" | "&gt;="
///   value    → STRING | NUMBER | "true" | "false"
/// </code>
///
/// <para><b>Type coercion:</b>
/// Numeric comparisons convert both sides to double. String comparisons
/// are case-sensitive. Null fields always fail the comparison (return false).</para>
/// </summary>
public static class FilterParser
{
    public static IFilterNode Parse(string expression)
    {
        var tokens = Tokenize(expression);
        int pos = 0;
        var result = ParseOrExpr(tokens, ref pos);

        if (pos < tokens.Count)
            throw new FormatException(
                $"Unexpected token at position {pos}: '{tokens[pos].Value}'");

        return result;
    }

    // ── Or expression ───────────────────────────────────────────────
    private static IFilterNode ParseOrExpr(List<Token> tokens, ref int pos)
    {
        var left = ParseAndExpr(tokens, ref pos);
        while (pos < tokens.Count && tokens[pos] is { Type: TokenType.Keyword, Value: "OR" })
        {
            pos++; // consume OR
            var right = ParseAndExpr(tokens, ref pos);
            left = new OrNode(left, right);
        }
        return left;
    }

    // ── And expression ──────────────────────────────────────────────
    private static IFilterNode ParseAndExpr(List<Token> tokens, ref int pos)
    {
        var left = ParseUnary(tokens, ref pos);
        while (pos < tokens.Count && tokens[pos] is { Type: TokenType.Keyword, Value: "AND" })
        {
            pos++; // consume AND
            var right = ParseUnary(tokens, ref pos);
            left = new AndNode(left, right);
        }
        return left;
    }

    // ── Unary (NOT) ─────────────────────────────────────────────────
    private static IFilterNode ParseUnary(List<Token> tokens, ref int pos)
    {
        if (pos < tokens.Count && tokens[pos] is { Type: TokenType.Keyword, Value: "NOT" })
        {
            pos++; // consume NOT
            var inner = ParseUnary(tokens, ref pos);
            return new NotNode(inner);
        }
        return ParsePrimary(tokens, ref pos);
    }

    // ── Primary (comparison or grouped) ─────────────────────────────
    private static IFilterNode ParsePrimary(List<Token> tokens, ref int pos)
    {
        if (pos >= tokens.Count)
            throw new FormatException("Unexpected end of filter expression");

        // Parenthesized group
        if (tokens[pos].Type == TokenType.LParen)
        {
            pos++; // consume (
            var expr = ParseOrExpr(tokens, ref pos);
            if (pos >= tokens.Count || tokens[pos].Type != TokenType.RParen)
                throw new FormatException("Missing closing parenthesis");
            pos++; // consume )
            return expr;
        }

        // Comparison: field op value
        if (tokens[pos].Type != TokenType.Identifier)
            throw new FormatException($"Expected field name, got '{tokens[pos].Value}'");

        string field = tokens[pos].Value;
        pos++;

        if (pos >= tokens.Count || tokens[pos].Type != TokenType.Operator)
            throw new FormatException($"Expected operator after '{field}'");

        var op = ParseOp(tokens[pos].Value);
        pos++;

        if (pos >= tokens.Count)
            throw new FormatException($"Expected value after operator");

        object value = ParseValue(tokens[pos]);
        pos++;

        return new ComparisonNode(field, op, value);
    }

    private static ComparisonOp ParseOp(string op) => op switch
    {
        "==" => ComparisonOp.Eq,
        "!=" => ComparisonOp.Neq,
        "<" => ComparisonOp.Lt,
        "<=" => ComparisonOp.Lte,
        ">" => ComparisonOp.Gt,
        ">=" => ComparisonOp.Gte,
        _ => throw new FormatException($"Unknown operator: {op}")
    };

    private static object ParseValue(Token token) => token.Type switch
    {
        TokenType.String => token.Value,
        TokenType.Number => double.TryParse(token.Value, out double d) ? d : token.Value,
        TokenType.Keyword when token.Value == "true" => true,
        TokenType.Keyword when token.Value == "false" => false,
        TokenType.Keyword when token.Value == "null" => null,
        _ => throw new FormatException($"Expected value, got '{token.Value}'")
    };

    // ── Tokenizer ───────────────────────────────────────────────────

    private static List<Token> Tokenize(string input)
    {
        var tokens = new List<Token>();
        int i = 0;

        while (i < input.Length)
        {
            char c = input[i];

            // Skip whitespace
            if (char.IsWhiteSpace(c)) { i++; continue; }

            // Parentheses
            if (c == '(') { tokens.Add(new Token(TokenType.LParen, "(")); i++; continue; }
            if (c == ')') { tokens.Add(new Token(TokenType.RParen, ")")); i++; continue; }

            // Operators (2-char first, then 1-char)
            if (i + 1 < input.Length)
            {
                string two = input.Substring(i, 2);
                if (two is "==" or "!=" or "<=" or ">=")
                {
                    tokens.Add(new Token(TokenType.Operator, two));
                    i += 2;
                    continue;
                }
            }
            if (c is '<' or '>')
            {
                tokens.Add(new Token(TokenType.Operator, c.ToString()));
                i++;
                continue;
            }

            // String literal
            if (c == '"' || c == '\'')
            {
                char quote = c;
                i++; // skip opening quote
                int start = i;
                while (i < input.Length && input[i] != quote) i++;
                tokens.Add(new Token(TokenType.String, input[start..i]));
                if (i < input.Length) i++; // skip closing quote
                continue;
            }

            // Number
            if (char.IsDigit(c) || (c == '-' && i + 1 < input.Length && char.IsDigit(input[i + 1])))
            {
                int start = i;
                if (c == '-') i++;
                while (i < input.Length && (char.IsDigit(input[i]) || input[i] == '.')) i++;
                tokens.Add(new Token(TokenType.Number, input[start..i]));
                continue;
            }

            // Identifier or keyword
            if (char.IsLetter(c) || c == '_')
            {
                int start = i;
                while (i < input.Length && (char.IsLetterOrDigit(input[i]) || input[i] == '_')) i++;
                string word = input[start..i];

                if (word is "AND" or "OR" or "NOT" or "true" or "false" or "null")
                    tokens.Add(new Token(TokenType.Keyword, word));
                else
                    tokens.Add(new Token(TokenType.Identifier, word));
                continue;
            }

            throw new FormatException($"Unexpected character '{c}' at position {i}");
        }

        return tokens;
    }

    private enum TokenType { Identifier, Operator, String, Number, Keyword, LParen, RParen }

    private readonly record struct Token(TokenType Type, string Value);
}
