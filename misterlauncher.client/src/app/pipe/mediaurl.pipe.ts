import { Pipe, PipeTransform, inject } from '@angular/core';
import { AuthService } from '../services/auth.service';

@Pipe({
  name: 'mediaurl',
  standalone: true
})
export class MediaurlPipe implements PipeTransform {

  constructor(private auth: AuthService) { }
 

  transform(value?: string): string {
    
    var t = this.auth.GetToken(); 
    //console.debug(`Token in pipe : ${t} [${this.auth.usertype}]`);
    return `api/media/${value}?token=${this.auth.GetToken()}`;
  }

}
