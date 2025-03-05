import { HttpClient, HttpEvent, HttpHandler, HttpHandlerFn, HttpHeaders, HttpInterceptor, HttpRequest } from '@angular/common/http';
import { Injectable, OnInit } from '@angular/core';
import { Observable } from 'rxjs';
import { Provider } from '@angular/core';
import { LoginResult } from './models/login-result';
import { JwtHelperService } from '@auth0/angular-jwt';
import { Router } from '@angular/router';
import md5 from 'md5';
import { GuestAccess } from './models/guest-access';

    

@Injectable({
  providedIn: 'root'
})
export class AuthService implements OnInit {

  constructor(private http: HttpClient, private router : Router) { }

  usertype: string = 'unknow'
  apistate: string = 'ERROR';

  ngOnInit(): void {
    console.log("## Auth Service init t ##");
    this.CheckTokenStored();
    }

  api_login(password: string) : Observable<LoginResult> {
    var payload = `{"password" : "${password}" }`;
    let headers = new HttpHeaders();
    headers = headers.set('Content-Type', 'application/json');
    return this.http.post<LoginResult>(`api/Auth/login`, payload, { headers: headers });
  }

  api_loginwithoutauthentication(): Observable<LoginResult> {
    let headers = new HttpHeaders();
    headers = headers.set('Content-Type', 'application/json');
    return this.http.get<LoginResult>(`api/Auth/loginwithoutauthentication`, { headers: headers });
  }

  api_requestguestaccess(key : string): Observable<GuestAccess> {
    let headers = new HttpHeaders();
    console.log(`key = ${key} Ascii = ${this.md5ToAscii(key)}`)
    var payload = `{ "signature" : "${this.md5ToAscii(key)}"}`;

    headers = headers.set('Content-Type', 'application/json');
    return this.http.post<GuestAccess>(`api/Auth/requestguestaccess`, payload, { headers: headers });
  }

  api_guestaccessconsumed(code: string, key: string): Observable<LoginResult> {
    let headers = new HttpHeaders();
    var payload = `{
      "code" : "${code}",
      "key" : "${key}"
      }`;

    headers = headers.set('Content-Type', 'application/json');
    return this.http.post<LoginResult>(`api/Auth/guestaccessconsumed`, payload, { headers: headers });
  }

  api_guestaccessstate(code: string): Observable<string> {
    let headers = new HttpHeaders();
    var payload = `{
      "code" : "${code}"      
      }`;

    headers = headers.set('Content-Type', 'application/json');
    return this.http.post<string>(`api/Auth/guestaccessstate`, payload, { headers: headers });
  }

  api_guestaccesscurrent(): Observable<GuestAccess[]> {
    let headers = new HttpHeaders();
    headers = headers.set('Content-Type', 'application/json');
    return this.http.get<GuestAccess[]>(`api/Auth/guestaccesscurrent`, { headers: headers });
  }

  api_guestaccessaction(code: string, approuved : Boolean): Observable<Boolean> {
    let headers = new HttpHeaders();
    var payload = `{
      "code" : "${code}",
      "approuved" : ${approuved} 
      }`;

    headers = headers.set('Content-Type', 'application/json');
    return this.http.post<Boolean>(`api/Auth/GuestAccessAction`, payload, { headers: headers });
  }





  SaveToken(loginresult: LoginResult) {
    console.debug("== SAVE token ==");
        
    const helper = new JwtHelperService();
    if (!helper.isTokenExpired(loginresult.token)) {
      localStorage.setItem('access_token', loginresult.token);
      const decodedToken = helper.decodeToken(loginresult.token);
      this.usertype = decodedToken.role;
      console.debug('Decode Token');
      console.debug(decodedToken)
    }
  }


    CheckTokenStored()
    {
      const token = localStorage.getItem('access_token');
      if (token == undefined || token == "") {
        this.usertype = "unknow";
        return;
      }
      const helper = new JwtHelperService();

      if (helper.isTokenExpired(token)) {
        localStorage.removeItem('access_token');
        this.usertype = "unknow";
        return;
      }

      const decodedToken = helper.decodeToken(token);
      this.usertype = decodedToken.role;   
    
       
  }

  Logout() {
    localStorage.removeItem('access_token');
    this.usertype = "unknow";
    this.router.navigateByUrl('/login')
  }
  


  GetToken() : string|null {
   return localStorage.getItem('access_token')
  }

  setApiState(value: string) {
    this.apistate = value;
  }

  generatekey(length : number) : string {
    let result = '';
    const characters = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789';
    const charactersLength = characters.length;
    let counter = 0;
    while (counter < length) {
      result += characters.charAt(Math.floor(Math.random() * charactersLength));
      counter += 1;
    }   
    return result;
  }

  md5ToAscii(input: string): string {
    
    const hash = md5(input);
    return hash;

  }




}

export function authInterceptor(req: HttpRequest<unknown>, next: HttpHandlerFn) {
  // Inject the current `AuthService` and use it to get an authentication token:
  const authToken = localStorage.getItem('access_token');
  if (authToken) {
    // Clone the request to add the authentication header.
    const newReq = req.clone({
      headers: req.headers.append('Authorization', `Bearer ${authToken}`),
    });
    return next(newReq);
  }
  else {
    return next(req);
  }
}





