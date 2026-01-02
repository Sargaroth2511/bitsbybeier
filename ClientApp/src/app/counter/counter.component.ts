import { Component, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';

@Component({
  selector: 'app-counter-component',
  templateUrl: './counter.component.html',
  standalone: true,
  imports: [MatButtonModule]
})
export class CounterComponent {
  currentCount = signal(0);

  incrementCounter() {
    this.currentCount.update(count => count + 1);
  }
}
