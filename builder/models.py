
import uuid

from django.conf import settings
from django.core.validators import URLValidator
from django.db import models


class DesktopProject(models.Model):
    class Status(models.TextChoices):
        DRAFT = "DRAFT", "Draft"
        READY = "READY", "Ready"
        BUILDING = "BUILDING", "Building"
        COMPLETED = "COMPLETED", "Completed"
        FAILED = "FAILED", "Failed"

    id = models.UUIDField(
        primary_key=True,
        default=uuid.uuid4,
        editable=False,
    )
    owner = models.ForeignKey(
        settings.AUTH_USER_MODEL,
        on_delete=models.CASCADE,
        related_name="desktop_projects",
    )
    name = models.CharField(max_length=80)
    website_url = models.URLField(max_length=2048, validators=[URLValidator()])
    logo = models.ImageField(upload_to="project-logos/")
    app_icon = models.ImageField(
        upload_to="project-icons/",
        blank=True,
    )
    brand_color = models.CharField(max_length=7, default="#173A63")
    fullscreen = models.BooleanField(default=False)
    status = models.CharField(
        max_length=20,
        choices=Status.choices,
        default=Status.DRAFT,
    )
    created_at = models.DateTimeField(auto_now_add=True)
    updated_at = models.DateTimeField(auto_now=True)

    def __str__(self):
        return self.name
