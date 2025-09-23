import { Component } from '@angular/core';
import { ProfileCardDto } from './../../../models/angular-course/profileCard.dto';
import { FormsModule } from '@angular/forms';
@Component({
  selector: 'app-profile-card',
  imports: [FormsModule],

  templateUrl: './profile-card.html',
  styleUrl: './profile-card.scss',
})
export class ProfileCard {
  profile: ProfileCardDto = {
    name: 'John Doe',
    age: 30,
    description: 'A passionate full stack developer',
  };
}
