import { Component, Input, OnInit } from '@angular/core';

@Component({
  selector: 'app-container',
  templateUrl: './container.component.html',
  styleUrls: ['./container.component.scss']
})
export class ContainerComponent implements OnInit {

  @Input() title: string;
  @Input() subTitle?: string;
  @Input() icon?: string;

  constructor() { }

  ngOnInit(): void {
  }

}
