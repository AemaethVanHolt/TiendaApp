using System.ComponentModel.DataAnnotations;

namespace TiendaApp.Models.ViewModels
{
    public class MetodoPagoViewModel // Este modelo se utiliza para representar los detalles del método de pago en el proceso de compra
    {
        // Propiedad para el total
        public decimal Total { get; set; }
        
        // Métodos de pago disponibles QUE NO ESTÁN FUNCIONALES PORQUE EL PAGO SE HACE AUTOMÁTICAMENTE Y QUEDA PENDIENTE
        public const string MetodoTarjeta = "Tarjeta";
        public const string MetodoTransferencia = "Transferencia";
        public const string MetodoPaypal = "PayPal";
        public const string MetodoBizum = "Bizum";
        public const string MetodoContraReembolso = "ContraReembolso";
        
        [Required(ErrorMessage = "El método de pago es obligatorio")]
        [Display(Name = "Método de pago")]
        public string MetodoPagoSeleccionado { get; set; } = string.Empty;
        
        [Display(Name = "Guardar información de pago")]
        public bool GuardarInfoPago { get; set; }
        
        // Propiedades para tarjeta de crédito
        [Display(Name = "Nombre del titular")]
        [RequiredIf("MetodoPagoSeleccionado", MetodoTarjeta, ErrorMessage = "El nombre del titular es obligatorio")]
        public string? NombreTitular { get; set; }
        
        [Display(Name = "Número de tarjeta")]
        [CreditCard(ErrorMessage = "El número de tarjeta no es válido")]
        [RequiredIf("MetodoPagoSeleccionado", MetodoTarjeta, ErrorMessage = "El número de tarjeta es obligatorio")]
        public string? NumeroTarjeta { get; set; }
        
        [Display(Name = "Vencimiento (MM/YY)")]
        [RegularExpression(@"^(0[1-9]|1[0-2])\/?([0-9]{2})$", ErrorMessage = "Formato inválido. Use MM/YY")] // Formato MM/YY
        [RequiredIf("MetodoPagoSeleccionado", MetodoTarjeta, ErrorMessage = "La fecha de vencimiento es obligatoria")]
        public string? FechaVencimiento { get; set; }
        
        [Display(Name = "CVV")]
        [RegularExpression(@"^[0-9]{3,4}$", ErrorMessage = "CVV inválido")]
        [RequiredIf("MetodoPagoSeleccionado", MetodoTarjeta, ErrorMessage = "El CVV es obligatorio")]
        public string? Cvv { get; set; }
        
        // Propiedades para transferencia bancaria
        [Display(Name = "Número de cuenta")]
        [RequiredIf("MetodoPagoSeleccionado", MetodoTransferencia, ErrorMessage = "El número de cuenta es obligatorio")]
        public string? NumeroCuenta { get; set; }
        
        // Propiedades para PayPal
        [Display(Name = "Correo de PayPal")]
        [EmailAddress(ErrorMessage = "Correo electrónico no válido")]
        [RequiredIf("MetodoPagoSeleccionado", MetodoPaypal, ErrorMessage = "El correo de PayPal es obligatorio")]
        public string? EmailPaypal { get; set; }
        
        // Propiedades para Bizum
        [Display(Name = "Teléfono para Bizum")]
        [Phone(ErrorMessage = "Número de teléfono no válido")]
        [RequiredIf("MetodoPagoSeleccionado", MetodoBizum, ErrorMessage = "El teléfono para Bizum es obligatorio")]
        public string? TelefonoBizum { get; set; }

        // Propiedades para contra reembolso. ESTO PAGA AUTOMÁTICAMENTE AL RECOGER EL PEDIDO
        [Display(Name = "Acepto los términos y condiciones")]
        [Range(typeof(bool), "true", "true", ErrorMessage = "Debe aceptar los términos y condiciones")]
        public bool AceptaTerminos { get; set; }
        
        // Propiedad para compatibilidad con la vista
        public bool GuardarParaFuturasCompras { get => GuardarInfoPago; set => GuardarInfoPago = value; }
    }
    
    // Clase auxiliar para validación condicional
    public class RequiredIfAttribute : ValidationAttribute
    {
        private string PropertyName { get; set; }
        private object DesiredValue { get; set; }

        public RequiredIfAttribute(string propertyName, object desiredValue, string errorMessage = "El campo es obligatorio")
        {
            PropertyName = propertyName;
            DesiredValue = desiredValue;
            ErrorMessage = errorMessage;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext context)
        {
            var instance = context.ObjectInstance;
            var type = instance.GetType();
            var propertyValue = type.GetProperty(PropertyName)?.GetValue(instance, null);
            
            if (propertyValue?.ToString() == DesiredValue.ToString() && (value == null || string.IsNullOrWhiteSpace(value.ToString())))
            {
                return new ValidationResult(ErrorMessage);
            }
            
            return ValidationResult.Success;
        }
    }
}