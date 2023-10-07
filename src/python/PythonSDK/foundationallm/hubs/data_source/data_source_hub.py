from foundationallm.hubs import HubBase
from foundationallm.hubs.data_source import DataSourceRepository, DataSourceResolver

class DataSourceHub(HubBase):
    """The DataSourceHub is responsible for resolving data sources."""

    def __init__(self):
        self.repository = DataSourceRepository()
        self.resolver = DataSourceResolver()