import React, { useState, useEffect } from 'react';
import ReactDOM from 'react-dom';
import EditPanel from './Edit';
import Result from './Result';
import Filter from './Filter';
import WebAPI from '../../../module/WebAPI';
import qs from 'qs';
import moment from 'moment';

const Panel = () => {
    const [stateFilter, setFilter] = useState({
        'SelectSite': [],
        'StartDate': moment().add(-10, 'days').format('l'),
        'EndDate': moment().format('l'),
    });

    const [stateDatas, setDatas] = useState({
        PageInfo: null,
        Datas: null,
        FilterInfo: null
    });
    const [editMode, setEdit] = useState(false);
    const [detailDatas, setDetail] = useState(null);

    const [siteList, setSiteList] = useState(null);

    useEffect(() => {
        let param = {
            Url: 'Common/AdminUserSiteList'
        };
        WebAPI.PostData(param).then((res) => {
            if (res.data.errorCode === 0) {
                setSiteList(res.data.data);
            } else {
                alert(res.data.message ?? 'failed to get data.');
            }
        })
    }, []);

    const Filter_OnChange = (v, dataColumn) => {
        setFilter({
            ...stateFilter,
            [dataColumn]: v
        });
    }

    const Filter_CheckBox_OnChange = (v, dataColumn) => {
        let isRemove = stateFilter[dataColumn].includes(v);
        let newChecked = isRemove
            ? stateFilter[dataColumn].filter(value => value !== v)
            : stateFilter[dataColumn].push(v);
        let currentTab = stateFilter[dataColumn];
        if (newChecked.length === 0) {
            currentTab = [];
        } else if (currentTab === null) {
            currentTab = newChecked;
        }

        if (isRemove && !newChecked.includes(currentTab)) {
            currentTab = newChecked.length === 0 ? [] : newChecked;
        }
        setFilter({
            ...stateFilter,
            [dataColumn]: currentTab
        });
    }

    const Filter_CheckBox_CheckAll = (v, dataColumn, options) => {
        if (v !== "All") return false;
        let currentTab = (stateFilter[dataColumn].length === options.length)
            ? []
            : options.map(o => {
                return o.value
            })
        setFilter({
            ...stateFilter,
            [dataColumn]: currentTab
        });
    }

    const Filter_Submit = () => {
        if (stateFilter.SelectSite.length === 0) {
            alert("Please Choose at least one Site")
            return false;
        }
        let param = {
            Url: 'Message/GetBatchMessageList',
            Data: qs.stringify({
                SelectSite: stateFilter.SelectSite,
                StartDate: moment(stateFilter.StartDate).unix(),
                EndDate: moment(stateFilter.EndDate).add(1, 'days').unix() - 1,
                PageNumber: 1
            })
        };

        WebAPI.PostData(param).then((res) => {
            if (res.data.errorCode === 0) {
                setDatas({
                    ...stateDatas,
                    PageInfo: {
                        totalPages: res.data.totalPages,
                        pageNumber: 1,
                        rowCount: res.data.rowCount
                    },
                    Datas: [...res.data.data],
                    FilterInfo: {
                        ...stateFilter
                    }
                })
            } else {
                alert(res.data.message ?? 'failed to get data.');
            }
        })
    }

    const Result_ChangePage = (PageNumber) => {
        if (stateDatas.FilterInfo.SelectSite.length === 0) {
            alert("Please Choose at least one Site")
            return false;
        }
        let param = {
            Url: 'Message/GetBatchMessageList',
            Data: qs.stringify({
                SelectSite: stateDatas.FilterInfo.SelectSite,
                StartDate: moment(stateDatas.FilterInfo.StartDate).unix(),
                EndDate: moment(stateDatas.FilterInfo.EndDate).add(1, 'days').unix() - 1,
                PageNumber: PageNumber
            })
        };

        WebAPI.PostData(param).then((res) => {
            if (res.data.errorCode === 0) {
                setDatas({
                    ...stateDatas,
                    PageInfo: {
                        ...stateDatas.PageInfo,
                        pageNumber: PageNumber
                    },
                    Datas: [...res.data.data],
                });
            } else {
                alert(res.data.message ?? 'failed to get data.');
            }
        })
    }

    const ClickEdit = id => {
        let param = {
            Url: 'Message/GetBatchMessage',
            Data: qs.stringify({ batchId: id })
        };
        WebAPI.PostData(param).then((res) => {
            if (res.data.errorCode === 0) {
                setDetail(res.data.data);
                SetEditMode(true);
            } else {
                alert(res.data.message ?? 'failed to get data.');
            }
        })
    }
    const ClickDelete = id => {
        if (window.confirm("Are you sure to delete data ?")) {
            let param = {
                Url: 'Message/DeleteBatchMessage',
                Data: qs.stringify({
                    SelectSite: stateFilter.SelectSite,
                    StartDate: moment(stateFilter.StartDate).unix(),
                    EndDate: moment(stateFilter.EndDate).add(1, 'days').unix() - 1,
                    PageNumber: 1,
                    batchId: id
                })
            };

            WebAPI.PostData(param).then((res) => {
                if (res.data.errorCode === 0) {
                    setDatas({
                        ...stateDatas,
                        PageInfo: {
                            totalPages: res.data.totalPages,
                            pageNumber: 1,
                            rowCount: res.data.rowCount
                        },
                        Datas: [...res.data.data],
                        FilterInfo: {
                            ...stateFilter
                        }
                    })
                } else {
                    alert(res.data.message ?? 'failed to get data.');
                }
            })
        }
    }

    const SetEditMode = mode => {
        if (mode === false) {
            setDetail(null);
        }
        setEdit(mode);
    }
    return (
        <div className="container-fluid">
            <h4 className="page-name">Personal Message Batch</h4>
            <div className="filter">
                <div className="row-flex">
                    <div className="button-group">
                        <button className="btn btn-search" type="submit" onClick={SetEditMode.bind(null, true)} > Add New</button>
                    </div>
                </div>
            </div>
            <Filter filterInfo={stateFilter} onChange={Filter_OnChange} onChangeCheckBox={Filter_CheckBox_OnChange} onChangeAllCheckBox={Filter_CheckBox_CheckAll} ClickSubmit={Filter_Submit} SiteList={siteList} />
            {
                editMode === false ?
                    stateDatas.Datas === null ? null : <Result {...stateDatas} ChangePage={Result_ChangePage} ClickEdit={ClickEdit} ClickDelete={ClickDelete} />
                    :
                    <EditPanel {...stateDatas} DetailDatas={detailDatas} SiteList={siteList} SetEditMode={SetEditMode} FilterSubmit={Filter_Submit} />
            }
        </div>
    )
}

export default Panel;

if (process.env.NODE_ENV === 'production') {
    ReactDOM.render(<Panel />, document.getElementById('content'))
}