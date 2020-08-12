import React from "react";
import Pagination from '../../../component/Pagination';
import moment from 'moment';
import { number_format, number_toFixed } from '../../../module/Formatting/NumberFormat';

const StatusData = {
    0: "Success",
    2: "Fail"
}
const Panel = (props) => {
    let dataRows = props.Datas.length === 0 ? <tr></tr> : props.Datas.map((data, idx) => {
        console.log(data);
        console.log(idx);
        return (
            <tr key={`result-body-${idx}`}>
                <td>{data.siteName}</td>
                <td>{data.tpName}</td>
                <td>{data.transId}</td>
                <td>{props.AdjustmentOptionData[data.direction]}</td>
                <td>{number_format(number_toFixed(data.amount, 2))}</td>
                <td>{StatusData[data.status]}</td>
                <td>{data.memo}</td>
                <td>{data.operator}</td>
                <td>{data.remarks}</td>
                <td>{moment(data.createTime).format('l LTS')}</td>
            </tr>
        )
    });

    let headerRows = props.Datas.length === 0 ? null : (
        <>
            <th scope="col">Site Name</th>
            <th scope="col">3RD Name</th>
            <th scope="col">Trans Id</th>
            <th scope="col">Adjust Type</th>
            <th scope="col">Adjust Amount</th>
            <th scope="col">Status</th>
            <th scope="col">Trans Memo</th>
            <th scope="col">Operator</th>
            <th scope="col">Remarks</th>
            <th scope="col">Create Time</th>
        </>
    );

    return (
        <div className="table">
            <table className="table table-striped">
                <thead>
                    <tr>
                        {headerRows}
                    </tr>
                </thead>
                <tbody>
                    {dataRows}
                </tbody>
            </table>
            {
                props.Datas.length === 0 ? null : (
                    <Pagination
                        Data={{
                            totalPages: props.PageInfo.totalPages,
                            pageNumber: props.PageInfo.pageNumber,
                            rowCount: props.PageInfo.rowCount
                        }}
                        ChangePage={props.ChangePage}
                    />
                )
            }
        </div>
    )
}

export default Panel;