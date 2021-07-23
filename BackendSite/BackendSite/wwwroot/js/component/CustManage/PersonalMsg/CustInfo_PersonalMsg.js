import { getUrlParameter } from '../../commJS/common';
import AddPersonalMsg from './AddPersonalMsg';
import { Pagination } from '../../commJS/Pagination.jsx';
import { columnDefs } from './dataDefs';
import ViewDetail from './DetailMsg';
import { NewPagination } from '../../commJS/NewPagination.jsx';

const page = {
    'main': 'main',
    'setting': 'setting',
    'detail': 'detail'
}

let custLang = '';
const PersonalMsg = (props) => {
    const [stateDatas, updateDatas] = React.useState({
        page: page.main,
        datas: null,
        PageInfo: {
            totalPages: 0,
            pageNumber: 1,
            rowCount: 0
        },
        detailData: null
    });

    const handlePage = (_page, viewData) => {
        if (_page === page.detail) {
            updateDatas({ ...stateDatas, page: _page, detailData: viewData });
        } else {
            updateDatas({ ...stateDatas, page: _page, detailData: null });
        }
    }

    React.useEffect(() => {
        let CustId = getUrlParameter("custId");
        let iCustId = parseInt(CustId);

        axios.post('/api/Message/GetCustLang', `CustId=${iCustId}`)
            .then(function (response) {
                let result = response.data;
                if (result.errorCode == 0) {
                    custLang = result.data.userLang
                } else {
                    alert(result.message);
                }
            }.bind(this)).catch(function (error) {
                alert(error);
            }.bind(this));

        axios.post('/api/Message/GetPersonalMsg', `CustId=${iCustId}&PageNumber=${1}`)
            .then(function (response) {
                let result = response.data;
                if (result.errorCode == 0) {
                    updateDatas({
                        ...stateDatas,
                        datas: result.data,
                        PageInfo: {
                            totalPages: result.totalPages,
                            pageNumber: 1,
                            rowCount: result.rowCount
                        }
                    });
                } else {
                    alert(result.message);
                }
            }.bind(this)).catch(function (error) {
                alert(error);
            }.bind(this));
    }, []);

    const Result_ChangePage = (PageNumber) => {
        let CustId = getUrlParameter("custId");
        let iCustId = parseInt(CustId);
        axios.post('/api/Message/GetPersonalMsg', `CustId=${iCustId}&PageNumber=${PageNumber}`)
            .then(function (response) {
                let result = response.data;
                if (result.errorCode == 0) {
                    updateDatas({
                        ...stateDatas,
                        datas: result.data,
                        PageInfo: {
                            totalPages: result.totalPages,
                            pageNumber: PageNumber,
                            rowCount: result.rowCount
                        }
                    });
                } else {
                    alert(result.message);
                }
            }.bind(this)).catch(function (error) {
                alert(error);
            }.bind(this));
    }

    const SendPersonalMsg = (datas) => {
        let CustId = getUrlParameter("custId");
        let iCustId = parseInt(CustId);
        let formData = new FormData();
        formData.append('CustId', parseInt(iCustId));
        formData.append('Title', datas.title);
        formData.append('Content', datas.content);
        formData.append('PageNumber', 1);
        axios.post('/api/Message/SendPersonalMsg', formData)
            .then(function (response) {
                let result = response.data;
                if (result.errorCode == 0) {
                    updateDatas({
                        ...stateDatas,
                        datas: result.data,
                        page: page.main,
                        PageInfo: {
                            totalPages: result.totalPages,
                            pageNumber: 1,
                            rowCount: result.rowCount
                        },
                        detailData: null
                    });
                } else {
                    alert(result.message);
                }
            }.bind(this)).catch(function (error) {
                alert(error);
            }.bind(this));
    }

    const DeleteMsg = (Id) => {
        if (window.confirm('delete this message?')) {
            let CustId = getUrlParameter("custId");
            let iCustId = parseInt(CustId);
            axios.post('/api/Message/DeletePersonalMsg', `CustId=${iCustId}&Id=${Id}&PageNumber=${stateDatas.PageInfo.pageNumber}`)
                .then(function (response) {
                    let result = response.data;
                    if (result.errorCode == 0) {
                        updateDatas({
                            ...stateDatas,
                            datas: result.data,
                            page: page.main,
                            PageInfo: {
                                ...stateDatas.PageInfo,
                                totalPages: result.totalPages,
                                rowCount: result.rowCount
                            },
                            detailData: null
                        });
                    } else {
                        alert(result.message);
                    }
                }.bind(this)).catch(function (error) {
                    alert(error);
                }.bind(this));
        }
    }

    let result = null;
    if (stateDatas.page === page.main) {
        let header = columnDefs.map((col, idx) => {
            return <th scope="col" key={`h-${idx}`}>{col.columnTitle}</th>
        });

        let bodyRows = stateDatas.datas === null ? [] : stateDatas.datas.map((data, idx) => {
            let row = columnDefs.map((element, idx) => {
                let subData = {};
                if (element.body && element.body.subData) {
                    element.body.subData.forEach(s => { subData[s] = data[s] });
                }

                return (
                    <td key={`td-${element.dataColumn}-${idx}`} className={element.body && element.body.attr && element.body.attr.class ? element.body.attr.class : ''}>
                        {element.body && element.body.format ? element.body.format(data[element.dataColumn], subData) : data[element.dataColumn]}
                    </td>
                );
            });

            return (
                <tr key={`result-body-${idx}`}>
                    {row}
                    <td>
                        <button className="btn btn-add" onClick={handlePage.bind(null, page.detail, data)}>view</button>
                        <button style={{ marginLeft: '.5rem' }} className="btn btn-delete" onClick={DeleteMsg.bind(null, data.id)}>delete</button>
                    </td>
                </tr>
            )
        });

        return (
            <>
                <div className="filter">
                    <button className="btn btn-add" onClick={handlePage.bind(null, page.setting)}>Add</button>
                </div>
                <div className="table">
                    <table className="table table-striped">
                        <thead>
                            <tr>
                                {header}
                                <th>Action</th>
                            </tr>
                        </thead>
                        <tbody>
                            {bodyRows}
                        </tbody>
                    </table>
                </div>
                <NewPagination
                    Data={{
                        totalPages: stateDatas.PageInfo.totalPages,
                        pageNumber: stateDatas.PageInfo.pageNumber,
                        rowCount: stateDatas.PageInfo.rowCount
                    }}
                    ChangePage={Result_ChangePage}
                />
            </>
        );
    } else if (stateDatas.page === page.setting) {
        return (
            <AddPersonalMsg ClickBack={handlePage.bind(null, page.main)} UserLang={custLang} ClickSend={SendPersonalMsg} />
        )
    } else {
        return (
            <ViewDetail data={stateDatas.detailData} ClickBack={handlePage.bind(null, page.main)} />
        )
    }
}

export default PersonalMsg;