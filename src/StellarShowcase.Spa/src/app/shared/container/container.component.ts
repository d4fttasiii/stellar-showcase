import { Component, Input } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-container',
  templateUrl: './container.component.html',
  styleUrls: ['./container.component.scss']
})
export class ContainerComponent {

  @Input() title: string;
  @Input() isLoading?: boolean;
  @Input() isSubmitting?: boolean;
  @Input() backUrl?: string;
  @Input() icon?: string;

  constructor(private router: Router) { }

  goBack() {
    this.router.navigate([this.backUrl]);
  }

}
