import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { IconModule, IconSetService } from '@coreui/icons-angular';
import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { GameDetailComponent } from './views/games/game-detail/game-detail.component';
import { SystemsComponent } from './views/systems/systems.component';
import { SystemDetailComponent } from './views/systems/system-detail/system-detail.component';
import { VideogamesComponent } from './views/videogames/videogames.component';
import { VideogameDetailComponent } from './views/videogames/videogame-detail/videogame-detail.component';
import { VideogamePlaylistComponent } from './views/videogames/videogame-playlist/videogame-playlist.component';
import { ItemlistSystemComponent } from './components/itemlist-system/itemlist-system.component';
import { ItemlistVideogameComponent } from './components/itemlist-videogame/itemlist-videogame.component';
import { MisterRemoteComponent } from './views/mister-remote/mister-remote.component';
import { MisterSettingsComponent } from './views/mister-settings/mister-settings.component';
import { JobScanComponent } from './views/job-scan/job-scan.component';
import { RomLinkComponent } from './views/job-scan/rom-link/rom-link.component';
import { MisterDatePipe } from './pipe/misterdate.pipe';
import { TimelapsePipe } from './pipe/timelapse.pipe';
import { TimeofdayPipe } from './pipe/timeofday.pipe';
import { FilesizePipe } from './pipe/filesize.pipe';
import { JobScanRomComponent } from './views/job-scan/job-scan-rom/job-scan-rom.component';
import { MisterstateColorPipe } from './pipe/misterstate-color.pipe';
import { JobstateColorPipe } from './pipe/jobstate-color.pipe';
import { MediaurlPipe } from './pipe/mediaurl.pipe';
import { ApierrorComponent } from './views/pages/apierror/apierror.component';
import { GuestAccessComponent } from './views/guest-access/guest-access.component';
import { MisterScriptComponent } from './views/mister-script/mister-script.component';

@NgModule({
  declarations: [
    AppComponent,
    GameDetailComponent,
    SystemsComponent,
    SystemDetailComponent,
    VideogamesComponent,
    VideogameDetailComponent,
    VideogamePlaylistComponent,
    ItemlistSystemComponent,
    ItemlistVideogameComponent,
    MisterRemoteComponent,
    MisterSettingsComponent,
    JobScanComponent,
    RomLinkComponent,
    MisterDatePipe,
    TimelapsePipe,
    TimeofdayPipe,
    FilesizePipe,
    JobScanRomComponent,
    MisterstateColorPipe,
    JobstateColorPipe,
    MediaurlPipe,
    ApierrorComponent,
    GuestAccessComponent,
    MisterScriptComponent
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    IconModule
  ],
  providers: [
    IconSetService   
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
