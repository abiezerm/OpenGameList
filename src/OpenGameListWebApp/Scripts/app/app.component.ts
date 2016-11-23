import {Component} from "@angular/core";

@Component({
    selector: 'opengamelist',
    template: `
        <h1>{{title}}</h1>
        <div class="menu">
            <a class="home" [routerLink]="['']">Home</a>
            |
            <a class="about" [routerLink]="['about']">About</a>
            |
            <a class="login" [routerLink]="['login']">Login</a>
        </div>
        <router-outlet></router-outlet>
    `,
    styles: [`
        item-list{
            min-width: 332px;
            border: 1px solid #aaaaaa;
            display: inline-block;
            margin: 0 10px;
            padding: 10px;
        }

        item-list.latest{
            background-color: #f9f9f9;
        }

        item-list.most-viewed {
            background-color: #f0f0f0;
        }

        item-list.random {
            background-color: #e9e9e9;
        }
    `]
})
export class AppComponent {
    title = "OpenGameList";
}