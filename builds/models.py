
import uuid

from django.conf import settings
from django.db import models


class BuildJob(models.Model):
    class Status(models.TextChoices):
        QUEUED = "QUEUED", "Queued"
        RUNNING = "RUNNING", "Running"
        SUCCEEDED = "SUCCEEDED", "Succeeded"
        FAILED = "FAILED", "Failed"

    id = models.UUIDField(
        primary_key=True,
        default=uuid.uuid4,
        editable=False,
    )
    project = models.ForeignKey(
        "builder.DesktopProject",
        on_delete=models.CASCADE,
        related_name="builds",
    )
    requested_by = models.ForeignKey(
        settings.AUTH_USER_MODEL,
        on_delete=models.CASCADE,
    )
    status = models.CharField(
        max_length=20,
        choices=Status.choices,
        default=Status.QUEUED,
    )
    log_text = models.TextField(blank=True)
    artifact = models.FileField(
        upload_to="build-artifacts/",
        blank=True,
    )
    created_at = models.DateTimeField(auto_now_add=True)
    finished_at = models.DateTimeField(null=True, blank=True)
