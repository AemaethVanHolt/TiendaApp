// Función para formatear moneda
function formatCurrency(amount) {
    return new Intl.NumberFormat('es-ES', { style: 'currency', currency: 'EUR' }).format(amount);
}

// Función para actualizar el contador del carrito, el del navbar
function updateCartCount() {
    fetch('/Carrito/GetCount')
        .then(response => {
            if (!response.ok) {
                throw new Error('Error al obtener la cantidad del carrito');
            }
            return response.json();
        })
        .then(data => {
            const cartCount = document.getElementById('cart-count');
            if (data.count > 0) {
                cartCount.textContent = data.count;
                cartCount.classList.remove('d-none');
            } else {
                cartCount.classList.add('d-none');
            }
        })
        .catch(error => {
            console.error('Error al actualizar el contador del carrito:', error);
        });
}

// Función para cargar los ítems del carrito
function loadCartItems() {
    console.log('Cargando ítems del carrito...');
    fetch('/Carrito/GetCartItems')
        .then(response => {
            if (!response.ok) {
                throw new Error('Error al cargar el carrito: ' + response.statusText);
            }
            return response.json();
        })
        .then(data => {
            console.log('Datos del carrito recibidos:', data);
            const cartItemsContainer = document.getElementById('cartDropdownItems');
            const cartTotalElement = document.getElementById('cartDropdownTotal');
            const cartCountElement = document.getElementById('cart-count');

            // Verificar si la respuesta indica éxito
            if (!data.success) {
                throw new Error(data.message || 'Error al cargar el carrito');
            }

            // Verificar si hay ítems en el carrito
            if (!data.items || data.items.length === 0) {
                cartItemsContainer.innerHTML = `
                    <div class="empty-cart text-center p-3">
                        <i class="fas fa-shopping-cart fa-3x mb-2 text-muted"></i>
                        <p class="mb-0">Tu carrito está vacío</p>
                    </div>`;
                cartTotalElement.textContent = formatCurrency(0);
                if (cartCountElement) {
                    cartCountElement.classList.add('d-none');
                }
                return;
            }

            // Actualizar el contador del carrito
            if (data.count > 0) {
                cartCountElement.textContent = data.count;
                cartCountElement.classList.remove('d-none');
            } else {
                cartCountElement.classList.add('d-none');
            }

            // Generar el HTML de los ítems del carrito
            let itemsHtml = '';
            data.items.forEach(item => {
                itemsHtml += `
                    <div class="dropdown-item cart-item" data-product-id="${item.productoId}">
                        <div class="row g-2 align-items-center">
                            <div class="col-3">
                                ${item.imagenUrl ? 
                                    `<img src="${item.imagenUrl}" alt="${item.nombre}" class="img-fluid rounded">` : 
                                    '<div class="no-image"><i class="fas fa-box"></i></div>'}
                            </div>
                            <div class="col-9">
                                <div class="d-flex justify-content-between align-items-start">
                                    <div class="me-2">
                                        <h6 class="mb-1">${item.nombre}</h6>
                                        <div class="d-flex align-items-center">
                                            <button class="btn btn-sm btn-outline-secondary btn-update-quantity" 
                                                    data-product-id="${item.productoId}" 
                                                    data-action="decrease">-</button>
                                            <input type="number" class="form-control form-control-sm quantity-input mx-1" 
                                                value="${item.cantidad}" min="1" 
                                                data-product-id="${item.productoId}" 
                                                style="width: 50px; text-align: center;">
                                            <button class="btn btn-sm btn-outline-secondary btn-update-quantity" 
                                                    data-product-id="${item.productoId}" 
                                                    data-action="increase">+</button>
                                        </div>
                                    </div>
                                    <div class="text-end">
                                        <div class="fw-bold">${formatCurrency(item.precio * item.cantidad)}</div>
                                        <small class="text-muted">${formatCurrency(item.precio)} c/u</small>
                                    </div>
                                </div>
                                <div class="text-end mt-1">
                                    <button class="btn btn-sm btn-link text-danger p-0 btn-remove-item" 
                                            data-product-id="${item.productoId}">
                                        <i class="fas fa-trash-alt"></i> Eliminar
                                    </button>
                                </div>
                            </div>
                        </div>
                    </div>
                    <div class="dropdown-divider"></div>`;
            });

            // Eliminar el último divisor
            itemsHtml = itemsHtml.replace(/<div class="dropdown-divider"><\/div>\s*$/, '');
            cartItemsContainer.innerHTML = itemsHtml;
            cartTotalElement.textContent = formatCurrency(data.total);

            // Agregar manejadores de eventos a los botones de actualizar cantidad
            document.querySelectorAll('.btn-update-quantity').forEach(button => {
                button.addEventListener('click', function(e) {
                    e.preventDefault();
                    e.stopPropagation();
                    
                    const productId = parseInt(this.getAttribute('data-product-id'));
                    const action = this.getAttribute('data-action');
                    const input = this.closest('.d-flex').querySelector('.quantity-input');
                    let quantity = parseInt(input.value) || 1;
                    
                    if (action === 'increase') {
                        quantity++;
                    } else if (action === 'decrease') {
                        quantity = Math.max(1, quantity - 1);
                    }
                    
                    input.value = quantity;
                    updateCartItemQuantity(productId, quantity);
                });
            });
            
            // Manejar cambios directos en el input
            document.querySelectorAll('.quantity-input').forEach(input => {
                input.addEventListener('change', function() {
                    const productId = parseInt(this.getAttribute('data-product-id'));
                    let quantity = parseInt(this.value) || 1;
                    quantity = Math.max(1, quantity); // Asegurar que la cantidad no sea menor a 1
                    this.value = quantity; // Actualizar el valor en caso de que sea inválido
                    updateCartItemQuantity(productId, quantity);
                });
            });

            // Agregar manejadores de eventos a los inputs de cantidad
            document.querySelectorAll('.quantity-input').forEach(input => {
                input.addEventListener('change', function() {
                    const productId = parseInt(this.getAttribute('data-product-id'));
                    const quantity = parseInt(this.value) || 1;
                    updateCartItemQuantity(productId, quantity);
                });
            });

            // Agregar manejadores de eventos a los botones de eliminar
            document.querySelectorAll('.btn-remove-item').forEach(button => {
                button.addEventListener('click', function() {
                    const productId = parseInt(this.getAttribute('data-product-id'));
                    removeFromCart(productId);
                });
            });
        })
        .catch(error => {
            console.error('Error al cargar los ítems del carrito:', error);
            const cartItemsContainer = document.getElementById('cartDropdownItems');
            cartItemsContainer.innerHTML = `
                <div class="text-center py-4">
                    <i class="fas fa-exclamation-triangle text-warning fa-2x mb-2"></i>
                    <p class="mb-0">Error al cargar el carrito. Por favor, intenta de nuevo.</p>
                </div>`;
        });
}

// Función para actualizar la cantidad de un ítem en el carrito
function updateCartItemQuantity(productId, quantity) {
    // Mostrar indicador de carga
    const itemElement = document.querySelector(`.cart-item[data-product-id="${productId}"]`);
    if (itemElement) {
        const input = itemElement.querySelector('.quantity-input');
        input.disabled = true;
        
        const buttons = itemElement.querySelectorAll('.btn-update-quantity');
        buttons.forEach(btn => btn.disabled = true);
    }
    
    fetch('/Carrito/UpdateQuantity', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value || ''
        },
        body: JSON.stringify({ productoId: productId, cantidad: quantity })
    })
    .then(response => {
        if (!response.ok) {
            return response.json().then(err => { 
                throw new Error(err.message || 'Error al actualizar la cantidad'); 
            });
        }
        return response.json();
    })
    .then(data => {
        if (data.success) {
            // Actualizar el contador del carrito
            updateCartCount();
            
            // Si el producto fue eliminado (cantidad = 0), recargar todo el carrito
            if (quantity <= 0) {
                loadCartItems();
                return;
            }
            
            // Actualizar el subtotal y total del ítem
            if (itemElement) {
                const subtotalElement = itemElement.querySelector('.item-subtotal');
                const totalElement = document.getElementById('cart-total');
                
                if (subtotalElement) {
                    subtotalElement.textContent = data.subtotal || formatCurrency(0);
                }
                
                if (totalElement) {
                    totalElement.textContent = data.total || formatCurrency(0);
                }
                
                // Actualizar el input
                const input = itemElement.querySelector('.quantity-input');
                if (input) {
                    input.value = quantity;
                }
                
                // Mostrar mensaje de éxito
                const messageElement = document.createElement('div');
                messageElement.className = 'alert alert-success alert-dismissible fade show mt-2';
                messageElement.innerHTML = `
                    Cantidad actualizada correctamente
                    <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
                `;
                
                const existingAlert = itemElement.querySelector('.alert');
                if (existingAlert) {
                    existingAlert.remove();
                }
                
                itemElement.appendChild(messageElement);
                
                // Ocultar el mensaje después de 3 segundos
                setTimeout(() => {
                    const alert = itemElement.querySelector('.alert');
                    if (alert) {
                        const bsAlert = new bootstrap.Alert(alert);
                        bsAlert.close();
                    }
                }, 3000);
            }
        } else {
            // Mostrar mensaje de error
            if (itemElement) {
                const messageElement = document.createElement('div');
                messageElement.className = 'alert alert-danger alert-dismissible fade show mt-2';
                messageElement.innerHTML = `
                    ${data.message || 'Error al actualizar la cantidad'}
                    <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
                `;
                
                const existingAlert = itemElement.querySelector('.alert');
                if (existingAlert) {
                    existingAlert.remove();
                }
                
                itemElement.appendChild(messageElement);
                
                // Ocultar el mensaje después de 5 segundos
                setTimeout(() => {
                    const alert = itemElement.querySelector('.alert');
                    if (alert) {
                        const bsAlert = new bootstrap.Alert(alert);
                        bsAlert.close();
                    }
                }, 5000);
            }
            
            // Recargar los ítems del carrito para restaurar los valores correctos
            loadCartItems();
        }
    })
    .catch(error => {
        console.error('Error al actualizar la cantidad:', error);
        
        // Mostrar mensaje de error
        if (itemElement) {
            const messageElement = document.createElement('div');
            messageElement.className = 'alert alert-danger alert-dismissible fade show mt-2';
            messageElement.innerHTML = `
                ${error.message || 'Error al actualizar la cantidad'}
                <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
            `;
            
            const existingAlert = itemElement.querySelector('.alert');
            if (existingAlert) {
                existingAlert.remove();
            }
            
            itemElement.appendChild(messageElement);
        }
        
        // Recargar los ítems del carrito para restaurar los valores correctos
        loadCartItems();
    })
    .finally(() => {
        // Habilitar los controles nuevamente
        if (itemElement) {
            const input = itemElement.querySelector('.quantity-input');
            if (input) input.disabled = false;
            
            const buttons = itemElement.querySelectorAll('.btn-update-quantity');
            buttons.forEach(btn => btn.disabled = false);
        }
    });
}

// Función para eliminar un ítem del carrito
function removeFromCart(productId) {
    if (!confirm('¿Estás seguro de que quieres eliminar este producto del carrito?')) {
        return;
    }

    fetch('/Carrito/RemoveItem', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': document.getElementById('RequestVerificationToken').value
        },
        body: JSON.stringify({ productoId: productId })
    })
    .then(response => {
        if (!response.ok) {
            return response.text().then(text => { throw new Error(text || 'Error al eliminar el producto'); });
        }
        return response.json();
    })
    .then(data => {
        if (data.success) {
            // Eliminar el elemento del DOM
            const itemElement = document.querySelector(`.cart-item[data-product-id="${productId}"]`);
            if (itemElement) {
                const divider = itemElement.nextElementSibling;
                if (divider && divider.classList.contains('dropdown-divider')) {
                    divider.remove();
                }
                itemElement.remove();
            }
            // Actualizar el contador y el total
            updateCartCount();
            // Si no quedan ítems, mostrar el mensaje de carrito vacío
            if (document.querySelectorAll('.cart-item').length === 0) {
                const cartItemsContainer = document.getElementById('cartDropdownItems');
                cartItemsContainer.innerHTML = `
                    <div class="empty-cart">
                        <i class="fas fa-shopping-cart"></i>
                        <p class="mb-0">Tu carrito está vacío</p>
                    </div>`;
                document.getElementById('cartDropdownTotal').textContent = formatCurrency(0);
            }
        }
    })
    .catch(error => {
        console.error('Error al eliminar el producto:', error);
        alert('Error al eliminar el producto del carrito');
        // Recargar los ítems del carrito para restaurar los valores correctos
        loadCartItems();
    });
}

// Cargar los ítems del carrito cuando se muestre el dropdown
document.addEventListener('DOMContentLoaded', function() {
    const cartDropdown = document.getElementById('cartDropdown');
    if (cartDropdown) {
        cartDropdown.addEventListener('shown.bs.dropdown', function() {
            loadCartItems();
        });
    }
    
    // Actualizar el contador del carrito al cargar la página
    updateCartCount();
});