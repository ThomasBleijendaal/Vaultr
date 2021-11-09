window.Interop = {
    addPopper: function (button, menu) {
        console.log(button);
        console.log(menu);

        var buttonElement = document.getElementById(button);
        var menuElement = document.getElementById(menu);

        console.log(buttonElement);
        console.log(menuElement);

        Popper.createPopper(buttonElement, menuElement, {
            placement: 'bottom',
        });

        return true;
    }
}