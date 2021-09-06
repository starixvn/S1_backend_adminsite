import { number_format } from '../../commJS/common';
import moment from "moment-timezone";
const Result = (props) => {
    let rows = props.Datas.length === 0 ? <tr><td>No Data.</td><td /><td /><td /><td /><td /><td /><td /><td /><td /><td /></tr> : props.Datas.map((d, dIdx) => {
        return (
            <tr key={`result-${dIdx}`}>
                <td>{moment(d.settledDate).format('l')}</td>
                <td>{number_format(d.categoryName)}</td>
                <td>{number_format(d.settledForValid_TurnOver + d.settledForValidNotRollover_TurnOver)}</td>
                <td>{number_format(d.settledForValid_TurnOver)}</td>
                <td>{number_format(d.settledForValidNotRollover_TurnOver)}</td>
                <td>{number_format(d.settledForValid_ValidBet + d.settledForValidNotRollover_ValidBet + d.other_ValidBet)}</td>
                <td>{number_format(d.settledForValid_ValidBet)}</td>
                <td>{number_format(d.settledForValidNotRollover_ValidBet)}</td>
                <td>{number_format(d.other_ValidBet)}</td>
                <td>{number_format(d.winlostAmount)}</td>
                <td><button className="btn btn-add" onClick={props.ClickDetail.bind(null, d.settledDate, d.categoryId)}>Details</button></td>
            </tr>
        )
    })
    return (
        <>
            <div className="table">
                <label class="col-form-label d-block">Statement By Day</label>
                <table className="table table-striped">
                    <thead>
                        <tr>
                            <th>Settled Date</th>
                            <th>Category Name</th>
                            <th>Turnover</th>
                            <th>Turnover (Settled)</th>
                            <th>Turnover (below odd)</th>
                            <th>Valid Bet</th>
                            <th>Valid Bet (Settled)</th>
                            <th>Valid Bet (below odd)</th>
                            <th>Valid Bet (UnSettled)</th>
                            <th>WinLoss</th>
                            <th>Action</th>
                        </tr>
                    </thead>
                    <tbody>
                        {rows}
                    </tbody>
                </table>
                <button type="button" className="btn btn-back" onClick={props.ClickBack.bind(null, "week")} style={{ marginTop: '1rem' }}>Back</button>
            </div>
        </>
    )
}

export default Result;