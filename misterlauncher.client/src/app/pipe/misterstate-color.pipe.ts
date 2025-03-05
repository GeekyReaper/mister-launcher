import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'misterstateColor',
  standalone: true
})
export class MisterstateColorPipe implements PipeTransform {

  transform(value: string): string {
    switch (value) {
      case 'OK':
        return "success";
      case 'WARNING':
        return "warning";
      default:
        return "danger";
    }
  }

}
