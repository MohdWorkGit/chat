import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';

export interface CustomRole {
  id: number;
  accountId: number;
  name: string;
  description: string | null;
  permissions: string[];
  createdAt: string;
  updatedAt: string;
}

export interface CreateCustomRoleRequest {
  accountId: number;
  name: string;
  description?: string | null;
  permissions?: string[];
}

export interface UpdateCustomRoleRequest {
  name: string;
  description?: string | null;
  permissions?: string[];
}

export interface SamlConfig {
  id: number;
  accountId: number;
  idpEntityId: string;
  idpSsoTargetUrl: string;
  idpCertificate: string;
  spEntityId: string;
  assertionConsumerServiceUrl: string;
  nameIdentifierFormat: string | null;
  enabled: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface SamlRoleMapping {
  id: number;
  samlConfigId: number;
  samlAttributeValue: string;
  userRole: string;
}

export interface CreateSamlRoleMappingRequest {
  samlAttributeValue: string;
  userRole: string;
}

@Injectable({
  providedIn: 'root',
})
export class EnterpriseService {
  private readonly api = inject(ApiService);

  // Custom Roles
  getCustomRoles(): Observable<CustomRole[]> {
    return this.api.get<CustomRole[]>(this.api.accountPath('/custom_roles'));
  }

  getCustomRole(id: number): Observable<CustomRole> {
    return this.api.get<CustomRole>(this.api.accountPath(`/custom_roles/${id}`));
  }

  createCustomRole(data: CreateCustomRoleRequest): Observable<CustomRole> {
    return this.api.post<CustomRole>(this.api.accountPath('/custom_roles'), data);
  }

  updateCustomRole(id: number, data: UpdateCustomRoleRequest): Observable<CustomRole> {
    return this.api.put<CustomRole>(this.api.accountPath(`/custom_roles/${id}`), data);
  }

  deleteCustomRole(id: number): Observable<void> {
    return this.api.delete<void>(this.api.accountPath(`/custom_roles/${id}`));
  }

  // SAML Config
  getSamlConfig(): Observable<SamlConfig> {
    return this.api.get<SamlConfig>(this.api.accountPath('/saml'));
  }

  createSamlConfig(data: Partial<SamlConfig>): Observable<SamlConfig> {
    return this.api.post<SamlConfig>(this.api.accountPath('/saml'), data);
  }

  updateSamlConfig(data: Partial<SamlConfig>): Observable<SamlConfig> {
    return this.api.put<SamlConfig>(this.api.accountPath('/saml'), data);
  }

  deleteSamlConfig(): Observable<void> {
    return this.api.delete<void>(this.api.accountPath('/saml'));
  }

  // SAML Role Mappings
  getSamlRoleMappings(): Observable<SamlRoleMapping[]> {
    return this.api.get<SamlRoleMapping[]>(this.api.accountPath('/saml/role_mappings'));
  }

  createSamlRoleMapping(data: CreateSamlRoleMappingRequest): Observable<SamlRoleMapping> {
    return this.api.post<SamlRoleMapping>(this.api.accountPath('/saml/role_mappings'), data);
  }

  deleteSamlRoleMapping(id: number): Observable<void> {
    return this.api.delete<void>(this.api.accountPath(`/saml/role_mappings/${id}`));
  }
}
