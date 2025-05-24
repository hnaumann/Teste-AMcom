namespace Questao5.Commons;

public sealed class ValidationException : Exception
{
    public string Tipo { get; }

    public ValidationException(string mensagem, string tipo) : base(mensagem)
    {
        Tipo = tipo;
    }
}