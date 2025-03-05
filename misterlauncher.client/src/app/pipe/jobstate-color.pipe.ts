import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'jobstateColor',
  standalone : true
})
export class JobstateColorPipe implements PipeTransform {

  transform(value: string): string {
    switch (value) {
     
      case 'RUNNING':
        return "info";
      case 'DONE':
        return "success";
      case 'CANCEL':
        return "warning";
      default:
        return "light";
    }
  }

}
