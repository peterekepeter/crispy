import api from "/api/crispy/es6";

// render todo items from C#
function reloadTodo() {
    api.todo.getAll().then(function (data) { // get all todo items from C#
        var container = document.querySelector('ul#my-todo');
        container.innerHTML = ''; // clear
        for (var i = 0; i < data.length; i++) {
            // render each item
            var item = document.createElement('li');
            item.textContent = data[i];
            container.appendChild(item);
        }
    });
}

function main(){
    // event handler for creating todo 
    document.querySelector('form#my-todo-add').addEventListener('submit', function (event) {
        event.preventDefault();
        var value = event.target.querySelector('input.item-text').value;
        api.todo.add(value).then(reloadTodo); // call C# method
        return false;
    });
    // initialize
    reloadTodo();
}

document.addEventListener("DOMContentLoaded", main);