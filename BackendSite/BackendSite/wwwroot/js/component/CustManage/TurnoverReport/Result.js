import {number_format} from '../../commJS/common';
const Result = (props)=>{
    let total = 0;
    let rows = props.Datas.length === 0 ? <tr><td>No Data.</td><td></td><td></td></tr> : props.Datas.map((d, dIdx)=>{
        total += d.stake;
        return(
            <tr key={`result-${dIdx}`}>
                <td>{d.userName}</td>
                <td>{d.gameName}</td>
                <td>{number_format(d.stake)}</td>
            </tr>
        )
    })
    return(
        <>
            <div className="table">
                <table className="table table-striped">
                    <thead>
                        <tr>
                            <th>Total</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr>
                            <td>{number_format(total)}</td>
                        </tr>
                    </tbody>
                </table>                
                <table className="table table-striped">
                    <thead>
                        <tr>
                            <th>UserName</th>
                            <th>GameName</th>
                            <th>Turnover</th>
                        </tr>
                    </thead>
                    <tbody>
                        {rows}
                    </tbody>
                </table>
            </div>
        </>
    )
}

export default Result;