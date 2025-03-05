import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'misterdatePipe',
  standalone: true
})
export class MisterDatePipe implements PipeTransform {

  formatter = new Intl.DateTimeFormat('fr', { month: 'short' });

  transform(datetoformat?: Date): string {
    if (datetoformat == undefined) {
      return "unknow";
    }
    let d = new Date(datetoformat);
    let month = d.getMonth() + 1;
    let day = d.getDate();
   
    if (day == 1 && month == 1) {
      
      return d.getFullYear().toString();
    }
    if (day == 1) {
      return this.formatter.format(d) + " " + d.getFullYear().toString();
    }

    return d.getDate().toString() + " " + this.formatter.format(d) + " " + d.getFullYear().toString();
   
  }

}
